using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace AlgebraSystem {
    public class Namespace {
        public string name;
        public static int namespaceIdx = 0;

        public Dictionary<string, Variable> variableLookup;
        public Dictionary<string, TypeSet> typeLookup;
        public Namespace parentNS;

        public Namespace(Namespace parentNS = null) {
            this.variableLookup = new Dictionary<string, Variable>();
            this.typeLookup = new Dictionary<string, TypeSet>();
            this.parentNS = parentNS;
            this.name = "namespace" + Namespace.namespaceIdx;
            Namespace.namespaceIdx++;
        }

        public Namespace DeepCopy() {
            Namespace copy = new Namespace(this.parentNS);
            foreach(var t in this.typeLookup.Keys) {
                copy.typeLookup.Add(t, this.typeLookup[t]);
            }
            foreach (var v in this.variableLookup.Keys) {
                copy.variableLookup.Add(v, this.variableLookup[v]);
            }
            return copy;
        }

        // ----- Checking contents -------------------------
        public bool ContainsVariableLocal(string name) {
            return this.variableLookup.ContainsKey(name);          
        }
        public bool ContainsTypeLocal(string name) {
            return this.typeLookup.ContainsKey(name);
        }
        public bool ContainsVariable(string name) {
            return this.VariableLookup(name) != null;
        }
        public bool ContainsType(string name) {
            return this.TypeLookup(name) != null;
        }
        
        // ----- Adding objects ----------------------------
        public bool AddTypeSet(string names) {
            if (names == "") return false;

            List<string> namesList = Parser.CssToList(names);
            bool success = true;
            foreach(var name in namesList) {
                string nameCapital = char.ToUpper(name[0]) + name.Substring(1);
                if (this.ContainsTypeLocal(nameCapital)) {
                    this.NameError(nameCapital);
                    return false;
                }
                this.typeLookup[nameCapital] = new TypeSet(nameCapital, this);
            }

            return success;
        }
        public bool AddTypeSet(string name, string regexString) {
            if (name == "") return false;            
            string nameCapital = char.ToUpper(name[0]) + name.Substring(1);
                
            if (this.ContainsTypeLocal(nameCapital)) {
                this.NameError(nameCapital);
                return false;
            }

            Regex regex;
            try {
                regex = new Regex(regexString);
            } catch(Exception) {
                Console.WriteLine("Error creating type " + name);
                Console.WriteLine("Badly formatted regex: " + regexString);
                return false;
            }
            
            this.typeLookup[nameCapital] = new TypeSet(nameCapital, this);
            this.typeLookup[nameCapital].constantRegex.Add(regex);

            return true;
        }

        public bool AddVariable(string names, TypeExpr typeExpr) {
            if (names == "" || typeExpr.typeTree == null || (typeExpr.typeTree.IsLeaf() && typeExpr.typeTree.value=="")) return false;

            if (!typeExpr.typeTree.ValidateAgainstNamespace(this)) {
                this.TypeTreeError(names, typeExpr.typeTree);
                return false;
            }

            List<string> namesList = Parser.CssToList(names);
            bool success = true;
            foreach (var name in namesList) {
                if (this.ContainsVariableLocal(name)) {
                    this.NameError(name);
                    success = false;
                }
                this.variableLookup.Add(name, new Variable(name, typeExpr, this, name));
            }

            return success;
        }
        public bool AddVariable(string names, string typeString) {
            TypeExpr typeExpr = new TypeExpr(typeString);
            return AddVariable(names, typeExpr);
        }


        // add primative constants
        public bool AddConstantPrimitive(string names, TypeExpr typeExpr) {
            if (names == "" || typeExpr.typeTree == null || (typeExpr.typeTree.IsLeaf() && typeExpr.typeTree.value == "")) return false;

            if (!typeExpr.typeTree.ValidateAgainstNamespace(this)) {
                this.TypeTreeError(names, typeExpr.typeTree);
                return false;
            }
            if(!typeExpr.typeTree.IsLeaf()) {
                Console.WriteLine("Cannot add ConstantPrimitive -- TypeTree is not primitive\n" + typeExpr.typeTree);
                return false;
            }

            List<string> namesList = Parser.CssToList(names);
            bool success = true;
            foreach (var name in namesList) {
                if (this.ContainsVariableLocal(name)) {
                    this.NameError(name);
                    success = false;
                }
                this.variableLookup.Add(name, new ConstantPrimitive(name, typeExpr, this, name));
            }

            return success;
        }
        public bool AddConstantPrimitive(string names, string typeName) {
            TypeExpr typeExpr = new TypeExpr(typeName);
            return AddConstantPrimitive(names, typeExpr);
        }

        // add lookup constant
        public bool AddConstantLookup(string name, Dictionary<string, string> lookup) {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }

            TypeExpr typeExpr = ConstantLookup.ValidateDictionary(lookup,this);
            if (typeExpr == null) {
                Console.WriteLine("Cannot add constant -- dictionary types are not valid.");
                return false;
            }

            this.variableLookup[name] = new ConstantLookup(name, typeExpr, this, lookup);
            return true;
        }

        public bool AddConstantConversion(string name, string typeTreeString, ConversionFuncs.ConvertMethod conversion) {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }
            if (conversion == null) {
                Console.WriteLine("Cannot add constant -- conversion function is not valid.");
                return false;
            }

            TypeExpr exprTree = new TypeExpr(typeTreeString);
            if (exprTree == null) {
                Console.WriteLine("Cannot add constant -- typeTree is not valid.");
                return false;
            }
            bool valid = exprTree.typeTree.ValidateAgainstNamespace(this);
            if (!valid) {
                Console.WriteLine("Cannot add constant -- typeTree is not valid.");
                return false;
            }

            this.variableLookup[name] = new ConstantConversion(name, exprTree, this, conversion);
            return true;
        }

        public bool AddConstantExpression(string name, Term expression, string vars="") {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }

            // "vars" specifies the order of the arguments which are BOUND (might also contain vars that don't appear in the expression)
            // all other variables are considered FREE
            List<string> boundVars = new HashSet<string>(Parser.CssToList(vars)).ToList(); // delete duplicates

            // Get a list of all used type variables
            List<string> usedTypeVars = new List<string>();
            foreach(var var in boundVars) {
                if (expression.ns.ContainsTypeLocal(var)) {
                    usedTypeVars.Concat(expression.ns.VariableLookup(var).typeExpr.typeTree.GetTypeVariables());
                }
            }
            // Get overall type tree
            List<TypeTree> treeList = new List<TypeTree>();
            foreach (var var in boundVars) {
                // variables not in the expression must be added (e.g. for "const x y = x") 
                if (!expression.ns.ContainsVariableLocal(var)) {
                    string typeVar = TypeTree.AddPrime("a", usedTypeVars);
                    expression.ns.AddVariable(var, typeVar);
                }
                treeList.Add(expression.ns.VariableLookup(var).typeExpr.typeTree);
            }
            treeList.Add(expression.typeTree);
            TypeTree typeTree = TypeTree.TypeTreeFromTreeList(treeList);
            TypeExpr typeExpr = new TypeExpr(typeTree);

            this.variableLookup.Add(name, new ConstantExpression(name, typeExpr, this, expression, boundVars));

            return true;
        }
        public bool AddConstantExpression(string name, string expressionString, string vars="") {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }

            Term expression = Term.TermFromSExpression(expressionString,this);
            if (expression == null) return false;

            return AddConstantExpression(name,expression,vars);            
        }
        // copies an existing variable from another namespace  into this namespace
        // (keeps the particular Constant subclass in tact; Evaluate() will work properly)
        public bool AddExistingVariable(Variable variable) {
            string name = variable.name;
            if (variable == null || this.variableLookup.ContainsKey(name)) return false;
            
            // make sure all relevant TypeSets exist so we can refer to them
            List<string> typeConstants = variable.typeExpr.typeTree.GetTypeConstants();
            foreach(var typeConst in typeConstants) {
                if(!this.typeLookup.ContainsKey(typeConst)) {
                    this.AddTypeSet(typeConst);
                }
            }

            this.variableLookup.Add(name, variable);
            return true;
        }


        // ----- Looking up objects ------------------------
        public Variable VariableLookup(string name) {
            Variable var = VariableLookupRegex(name);
            if (var != null) return var;

            if (this.variableLookup.ContainsKey(name)) {
                return this.variableLookup[name];
            } else if (this.parentNS != null) {
                return this.parentNS.VariableLookup(name);
            } else {
                return null;
            }
        }

        // create regex variables on-the-fly
        public Variable VariableLookupRegex(string name) {
            foreach(var typeset in typeLookup.Values) {
                foreach(var regex in typeset.constantRegex) {
                    if(regex.IsMatch(name)) {
                        return new ConstantPrimitive(name, typeset.name, this);
                    }   
                }
            }
            return null;
        }

        public TypeSet TypeLookup(string name) {
            if (this.typeLookup.ContainsKey(name)) {
                return this.typeLookup[name];
            } else if (this.parentNS != null) {
                return this.parentNS.TypeLookup(name);
            } else {
                return null;
            }
        }


        // ----- Errors ------------------------------------
        public void NameError(string name) {
            Console.WriteLine("Error creating name '" + name + "'");
            Console.WriteLine("Name is already used.");
        }
        public void TypeTreeError(string name, TypeTree typeTree) {
            Console.WriteLine("Error creating name '" + name + "'");
            Console.WriteLine("Type tree is not valid in this namespace: " + typeTree);
        }




        // ----- Global namespace types/variables ----------
        public static Namespace CreateGlobalNs() {
            Namespace gns = new Namespace();

            ///// BOOLEANS ///////////////
            gns.AddTypeSet("Bool");
            gns.AddConstantPrimitive("true,false", "Bool");

            Dictionary<string, string> AND = new Dictionary<string, string>() {
                { "true,true",  "true"},
                { "true,false", "false"},
                { "false,true", "false"},
                { "false,false","false"}
            };
            Dictionary<string, string> OR = new Dictionary<string, string>() {
                { "true,true",  "true"},
                { "true,false", "true"},
                { "false,true", "true"},
                { "false,false","false"}
            };
            Dictionary<string, string> XOR = new Dictionary<string, string>() {
                { "true,true",  "false"},
                { "true,false", "true"},
                { "false,true", "true"},
                { "false,false","false"}
            };
            Dictionary<string, string> NOT = new Dictionary<string, string>() {
                { "true",  "false"},
                { "false", "true"}
            };

            gns.AddConstantLookup("AND", AND);
            gns.AddConstantLookup("OR", OR);
            gns.AddConstantLookup("XOR", XOR);
            gns.AddConstantLookup("NOT", NOT);


            ///// INTEGERS ///////////////

            string intRegexString = "^-?(\\d+)$";
            gns.AddTypeSet("Int", intRegexString);

            gns.AddConstantConversion("+", "(Int -> (Int -> Int))", ConversionFuncs._IntAdd);
            gns.AddConstantConversion("-", "(Int -> (Int -> Int))", ConversionFuncs._IntSub);
            gns.AddConstantConversion("*", "(Int -> (Int -> Int))", ConversionFuncs._IntMult);
            gns.AddConstantConversion("/", "(Int -> (Int -> Int))", ConversionFuncs._IntDiv);
            gns.AddConstantConversion("%", "(Int -> (Int -> Int))", ConversionFuncs._IntMod);
            gns.AddConstantConversion("^", "(Int -> (Int -> Int))", ConversionFuncs._IntPow);

            gns.AddConstantConversion("=", "(Int -> (Int -> Bool))", ConversionFuncs._EQ);
            gns.AddConstantConversion("<", "(Int -> (Int -> Bool))", ConversionFuncs._LT);
            gns.AddConstantConversion(">", "(Int -> (Int -> Bool))", ConversionFuncs._GT);



            ///// STANDARD POLYMORPHIC FUNCTIONS ///////////////
            // creates namespaces 1-5
            gns.AddConstantExpression("id", "x", "x");
            gns.AddConstantExpression("apply", "f x", "f,x");
            gns.AddConstantExpression("const", "x", "x,y");
            gns.AddConstantExpression("cmp", "f (g x)", "f,g,x");
            gns.AddConstantExpression("homo", "op (f x) (f y)", "op,f,x,y");

            return gns;
        }

        public override string ToString() {
            return this.name;
        }
    }
}

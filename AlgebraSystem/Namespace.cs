using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace AlgebraSystem {
    public class Namespace {
        public static int namespaceIdx = 0;
        public string name;

        public Dictionary<string, Variable> variableLookup;
        public Dictionary<string, TypeSet> typeLookup;
        public Dictionary<string, TypeConstructor> typeConstructorLookup;
        public Dictionary<string, TypeClass> typeClassLookup;
        public Dictionary<string, TreeTransformation> transformationLookup;
        public Namespace parentNS;

        public Namespace(Namespace parentNS = null) {
            this.variableLookup = new Dictionary<string, Variable>();
            this.typeLookup = new Dictionary<string, TypeSet>();
            this.typeConstructorLookup = new Dictionary<string, TypeConstructor>();
            this.typeClassLookup = new Dictionary<string, TypeClass>();
            this.transformationLookup = new Dictionary<string, TreeTransformation>();
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
        public bool ContainsTypeConstructorLocal(string name) {
            return this.typeConstructorLookup.ContainsKey(name);
        }
        public bool ContainsTypeClassLocal(string name) {
            return this.typeClassLookup.ContainsKey(name);
        }
        public bool ContainsVariable(string name) {
            return this.VariableLookup(name) != null;
        }
        public bool ContainsType(string name) {
            return this.TypeLookup(name) != null;
        }
        public bool ContainsTypeConstructor(string name) {
            return this.TypeConstructorLookup(name) != null;
        }
        public bool ContainsTypeClass(string name) {
            return this.TypeClassLookup(name) != null;
        }

        public bool ValidateTree(TypeTree tree) {
            if (tree.IsLeaf()) {
                return TypeTree.IsTypeVariable(tree.value) || (this.TypeLookup(tree.value) != null);
            } else {
                return this.ValidateTree(tree.GetLeft()) && this.ValidateTree(tree.GetRight());
            }
        }

        // ----- Adding objects ----------------------------
        public bool AddTypeClass(string name, List<string> typeArgs, Dictionary<string,string> methodTypeStrings) {
            // confirm none of the proposed names exist in the namespace 
            foreach (var methodName in methodTypeStrings.Keys) {
                if (this.ContainsVariableLocal(methodName)) {
                    this.NameError(methodName);
                    return false;
                }
            }

            TypeClass typeClass = TypeClass.ParseTypeClass(name, typeArgs, methodTypeStrings, this);
            if (typeClass == null) return false;

            this.typeClassLookup.Add(name, typeClass);

            foreach(var v in methodTypeStrings.Keys) {
                var c = new ConstantOverloaded(v, methodTypeStrings[v], this);
                this.variableLookup.Add(v, c);
            }

            return true;
        }


        public bool AddTypeDefinition(string typeDefinitionString) {
            string[] halves = typeDefinitionString.Split('=');
            string typeConstructorString = halves[0].Trim();
            string valueConstructorString = null;
            if (halves.Length > 1) {
                valueConstructorString = halves[1];
            }

            int length = Parser.TypeIdentifier(typeConstructorString);
            string name = typeConstructorString.Substring(0, length);

            bool succ1 = this.AddTypeConstructor(typeConstructorString);
            if (!succ1) return false;
            if(valueConstructorString!=null) {
                bool succ2 = this.AddValueConstructor(valueConstructorString, name);
                if (!succ2) {
                    this.typeConstructorLookup.Remove(name);
                    return false;
                }
            }

            return true;
        }

        public bool AddTypeConstructor(string tcString) {
            int typeNameLength = Parser.TypeIdentifier(tcString);
            string typeNameString = tcString.Substring(0, typeNameLength);
            if(ContainsTypeConstructorLocal(typeNameString)) {
                this.NameError(typeNameString);
                return false;
            }

            TypeConstructor tc = TypeConstructor.ParseTypeConstructor(tcString, this);
            if (tc == null) return false;

            this.typeConstructorLookup.Add(typeNameString, tc);
            return true;           
        }

        public bool AddValueConstructor(string vcStringList, string tcName) {
            if (!this.ContainsTypeConstructorLocal(tcName)) {
                this.TcError(tcName);
                return false;
            }
            TypeTree resultTypeTree = this.TypeConstructorLookup(tcName).resultTypeTree;

            var vcList = new List<ValueConstructor>();
            var knownKinds = new Dictionary<string, KindTree>();
            List<string> valueConstructorStrings = vcStringList.Split('/').Select(x => x.Trim()).ToList();
            foreach (var vcString in valueConstructorStrings) {
                var vc = ValueConstructor.ParseValueConstructor(vcString, resultTypeTree, this, knownKinds);
                if (vc == null) return false;
                if (this.ContainsVariableLocal(vc.name)) {
                    this.NameError(vc.name);
                    return false;
                }
                vcList.Add(vc);
            }

            foreach(var vc in vcList) {
                this.variableLookup.Add(vc.name, vc);
            }            
            return true;
        }



        public bool AddTypeSet(string names) {
            if (names == "") return false;

            List<string> namesList = Parser.ScsvToList(names);
            bool success = true;
            foreach(var name in namesList) {
                string nameCapital = char.ToUpper(name[0]).ToString();
                if(name.Length > 1) nameCapital += name.Substring(1);
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

            if (!this.ValidateTree(typeExpr.typeTree)) {
                this.TypeTreeError(names, typeExpr.typeTree);
                return false;
            }

            List<string> namesList = Parser.ScsvToList(names);
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

            if (!this.ValidateTree(typeExpr.typeTree)) {
                this.TypeTreeError(names, typeExpr.typeTree);
                return false;
            }
            if(!typeExpr.typeTree.IsLeaf()) {
                Console.WriteLine("Cannot add ConstantPrimitive -- TypeTree is not primitive\n" + typeExpr.typeTree);
                return false;
            }

            List<string> namesList = Parser.ScsvToList(names);
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
            bool valid = this.ValidateTree(exprTree.typeTree);
            if (!valid) {
                Console.WriteLine("Cannot add constant -- typeTree is not valid.");
                return false;
            }

            this.variableLookup[name] = new ConstantConversion(name, exprTree, this, conversion);
            return true;
        }

        public bool AddConstantExpression(string name, TermNew expression, string vars="") {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }

            Namespace expressionNS = new Namespace(this);

            // "vars" specifies the order of the arguments which are BOUND (might also contain vars that don't appear in the expression)
            // all other variables are considered FREE
            List<string> boundVars = new HashSet<string>(Parser.ScsvToList(vars)).ToList(); // delete duplicates
            Dictionary<string, TypeTree> namesToTypes = expression.GetNamesToTypesDictionary();

            // Get a list of all used type variables for variables present in the expression
            List<string> boundTypeVars = new List<string>();
            foreach(var var in boundVars) {
                if (namesToTypes.ContainsKey(var)) {
                    List<string> typeVars = namesToTypes[var].GetTypeVariables();
                    boundTypeVars.AddRange(typeVars);
                }
            }
            boundTypeVars = new HashSet<string>(boundTypeVars).ToList(); // delete duplicates

            // Make up type variables for variables not present in the expression
            // variables not in the expression must be added (e.g. for "const x y = x") 
            int idx = 0;
            foreach (var var in boundVars) {
                if (!namesToTypes.ContainsKey(var)) {
                    string newTypeVar = "t" + idx.ToString();
                    while(boundTypeVars.Contains(newTypeVar)) {
                        idx++;
                        newTypeVar = "t" + idx.ToString();
                    }
                    boundTypeVars.Add(newTypeVar);
                    namesToTypes.Add(var, TypeTree.MakePrimitiveTree(newTypeVar));
                    idx++;
                }
            }

            // Get overall type tree
            List<TypeTree> treeList = new List<TypeTree>();
            foreach (var var in boundVars) {
                TypeTree tTree = namesToTypes[var];
                TypeExpr tExpr = new TypeExpr(tTree,tTree.GetTypeVariables());
                expressionNS.AddVariable(var, tExpr);
                treeList.Add(tTree);
            }
            treeList.Add(expression.typeTree);
            TypeTree typeTree = TypeTree.TypeTreeFromTreeList(treeList);
            TypeExpr typeExpr = new TypeExpr(typeTree, boundTypeVars);

            this.variableLookup.Add(name, new ConstantExpression(name, typeExpr, this, expressionNS, expression, boundVars));

            return true;
        }
        public bool AddConstantExpression(string name, string expressionString, string vars="") {
            if (name == "" || this.ContainsVariableLocal(name)) {
                this.NameError(name);
                return false;
            }

            TermNew expression = TermNew.TermFromSExpression(expressionString,this);
            if (expression == null) return false;

            return AddConstantExpression(name, expression, vars);            
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

        // ----- Add Transformation ------------------------
        public bool AddTransformation(string name, TreeTransformation t) {
            if (this.transformationLookup.ContainsKey(name)) return false;
            this.transformationLookup.Add(name, t);
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

        public TypeConstructor TypeConstructorLookup(string name) {
            if (this.typeConstructorLookup.ContainsKey(name)) {
                return this.typeConstructorLookup[name];
            } else if (this.parentNS != null) {
                return this.parentNS.TypeConstructorLookup(name);
            } else {
                return null;
            }
        }

        public TypeClass TypeClassLookup(string name) {
            if (this.typeClassLookup.ContainsKey(name)) {
                return this.typeClassLookup[name];
            } else if (this.parentNS != null) {
                return this.parentNS.TypeClassLookup(name);
            } else {
                return null;
            }
        }


        public TreeTransformation TransformationLookup(string name) {
            if (this.transformationLookup.ContainsKey(name)) {
                return this.transformationLookup[name];
            } else if (this.parentNS != null) {
                return this.parentNS.TransformationLookup(name);
            } else {
                return null;
            }
        }

        // ----- Errors ------------------------------------
        public void NameError(string name) {
            Console.WriteLine("Error creating name '" + name + "'");
            Console.WriteLine("Name is already used.");
        }
        public void TcError(string name) {
            Console.WriteLine("Error creating value constructors for type'" + name + "'");
            Console.WriteLine("Type constructor does not exist.");
        }
        public void TypeTreeError(string name, TypeTree typeTree) {
            Console.WriteLine("Error creating name '" + name + "'");
            Console.WriteLine("Type tree is not valid in this namespace: " + typeTree);
        }


        // ----- Global namespace types/variables ----------
        public static Namespace CreateGlobalNs() {
            Namespace gns = new Namespace();

            ///// TYPE CONSTRUCTORS //////
            gns.AddTypeDefinition("-> a b");
            gns.AddTypeDefinition("| a b = Left a / Right b");
            gns.AddTypeDefinition(", a b = Pair a b");
            gns.AddTypeDefinition("List a = Cons a (List a) / Nil");
            gns.AddTypeDefinition("Boolean = True / False");

            gns.AddTypeSet("->");
            gns.AddTypeSet("|");
            gns.AddTypeSet(",");

            ///// BOOLEANS ///////////////
            gns.AddTypeSet("RGB");
            gns.AddConstantPrimitive("red;green;blue;", "RGB");

            ///// BOOLEANS ///////////////
            gns.AddTypeSet("Bool");
            gns.AddConstantPrimitive("true;false", "Bool");

            Dictionary<string, string> AND = new Dictionary<string, string>() {
                { "true;true",  "true"},
                { "true;false", "false"},
                { "false;true", "false"},
                { "false;false","false"}
            };
            Dictionary<string, string> OR = new Dictionary<string, string>() {
                { "true;true",  "true"},
                { "true;false", "true"},
                { "false;true", "true"},
                { "false;false","false"}
            };
            Dictionary<string, string> XOR = new Dictionary<string, string>() {
                { "true;true",  "false"},
                { "true;false", "true"},
                { "false;true", "true"},
                { "false;false","false"}
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
            gns.AddConstantConversion("neg", "(Int -> Int)", ConversionFuncs._IntNeg);

            gns.AddConstantConversion("=", "(Int -> (Int -> Bool))", ConversionFuncs._EQ);
            gns.AddConstantConversion("<", "(Int -> (Int -> Bool))", ConversionFuncs._LT);
            gns.AddConstantConversion(">", "(Int -> (Int -> Bool))", ConversionFuncs._GT);


            ///// STANDARD POLYMORPHIC FUNCTIONS ///////////////
            // creates namespaces 1-5
            gns.AddConstantExpression("id", "x", "x");
            gns.AddConstantExpression("apply", "f x", "f;x");
            gns.AddConstantExpression("const", "x", "x;y");
            gns.AddConstantExpression("cmp", "f (g x)", "f;g;x");
            gns.AddConstantExpression("homo", "op (f x) (f y)", "op;f;x;y");


            ///// STANDARD TREE TRANSFORMATIONS ////////////////
            gns.AddTransformation("DeMorgan1", 
                TreeTransformationFactory.MakeHomomorphism("NOT", "AND", "OR", gns));
            gns.AddTransformation("DeMorgan2",
                TreeTransformationFactory.MakeHomomorphism("NOT", "OR", "AND", gns));
            gns.AddTransformation("ExponentLaw",
                TreeTransformationFactory.MakeHomomorphism("^ b", "*", "+", gns));
            gns.AddTransformation("Factoring",
                TreeTransformationFactory.MakeHomomorphism("* a", "+", "+", gns));

            return gns;
        }

        public override string ToString() {
            return this.name;
        }
    }
}

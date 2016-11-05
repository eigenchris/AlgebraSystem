using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
namespace AlgebraSystemV4 {
    public class Node {

        public string functionName;
        public TypeTree typeTree;
        public Namespace ns;
        public List<Node> children;
        public Node parent { get; }

        public Node(string fname, Namespace ns) {
            this.functionName = fname;
            this.typeTree = ns.VariableLookup(fname).typeTree.DeepCopy();
            this.ns = ns;

            this.children = new List<Node>();
            this.parent = null;
        }


        // ----- Apply and Eval -------------------------------
        public bool Apply(string args) {
            List<string> argsList = Parser.CssToList(args);
            bool success = true;
            foreach (var arg in argsList) {
                if (!ns.ContainsVariable(arg)) {
                    Console.WriteLine("Cannot add child: lookup of '" + arg + "' failed!");
                    return false;
                }
                Node n = new Node(arg, this.ns);
                success = success && Apply(n);
                if (!success) { break; }
            }
            return success;
        }

        // Apply does the following:
        // 1) ensure that the input type of that slot matches the type of the node 'n'
        // 2) update the type variables in the parent, if needed
        // 3) Add the child node
        // 4) update the parent node's type by popping off the first left branch of the type tree     
        public bool Apply(Node childNode) {
            // make sure that an input slot exists
            if (this.typeTree.IsPrimitive()) {
                Console.WriteLine("Cannot add child: no argument slots remaining!");
                return false;
            }

            // replace type variable names in the child tree as needed so that it shares no type vars with the parent
            TypeTree parentTree = this.typeTree.DeepCopy();
            TypeTree childTree = childNode.typeTree.DeepCopy();
            List<string> parentTypeVars = parentTree.GetTypeVariables();
            List<string> childTypeVars = childTree.GetTypeVariables();
            List<string> allTypeVars = new List<string>(parentTypeVars.Concat(childTypeVars));
            foreach (var childTypeVar in childTypeVars) {
                if (parentTypeVars.Contains(childTypeVar)) {
                    string newVar = TypeTree.AddPrime(childTypeVar,allTypeVars);
                    childTree.ReplaceName(childTypeVar, newVar);
                }
            }

            // type match
            Dictionary<string, TypeTree> typeVarDictionary = new Dictionary<string, TypeTree>();
            bool match = TypeTree.MatchPrototype(parentTree.GetLeft(), childTree, typeVarDictionary);
            if (!match) {
                Console.WriteLine("Input type of '" + childNode + "': " + childNode.typeTree + " does not match expected type of " + this.typeTree.GetLeft());
                return false;
            }

            // substitute type trees for type variables as needed
            foreach (var key in typeVarDictionary.Keys) {
                parentTree = parentTree.Substitute(key, typeVarDictionary[key]);
            }

            // Add child node to parent node
            this.children.Add(childNode);

            // update parent node's type
            this.typeTree = parentTree.GetRight();

            return true;
        }

        public void Eval() {
            // if function has enough arguments to evaluate, do so
            List<string> args = new List<string>();
            foreach (var child in this.children) {
                child.Eval();
                args.Add(child.functionName);
            }

            // do evaluation if all arguments are present; otherwise, do not
            Variable functionObj = this.ns.VariableLookup(this.functionName);
            int numberOfInputs = this.typeTree.GetNumberOfInputs();
            //if (functionObj.numberOfInputs == args.Count) { //inforrect; return type might not be # of inputs to expression!
            //if (numberOfInputs == args.Count) {
                Term result = functionObj.Evaluate(args);
                // empty string means evaluation failed; leave things as they are
                // don't need this for primatives, do we?
                if (result != null) {
                    this.functionName = result;
                    this.typeTree = this.ns.VariableLookup(result).typeTree;
                    this.children.Clear(); // NOT NULL! If you set it too null, we can't foreach loop over it
                }
            //}

        }

       
        // ----- Basic tree functionality ---------------------
        public bool IsLeaf() {
            return this.children.Count == 0;
        }

        public bool Contains(string name) {
            if (this.functionName == name) { return true; }
            bool success;
            foreach (var child in this.children) {
                success = child.Contains(name);
                if (success) { return true; }
            }
            return false;
        }

        public bool DeepEquals(Node n) {
            if (this.children.Count != n.children.Count) return false;
            bool success;
            for(int i=0; i<this.children.Count; i++) {
                success = this.children[i].DeepEquals(n.children[i]);
                if (!success) return false;
            }
            return true;
        }


        // get a list of all variables in the tree, in a pre-traversal
        public List<string> GetVariables(List<string> varsList = null) {
            varsList = varsList ?? new List<string>();
            // lookup will always succeed if node was added properly
            if (this.ns.VariableLookup(this.functionName).GetCompType() == Variable.ComputationType.variable
                && !varsList.Contains(this.functionName)) {
                varsList.Add(this.functionName);
            }
            foreach (var child in this.children) {
                child.GetVariables(varsList); // pass  by reference should keep all relevant vars
            }
            return varsList;
        }
        
        public Node Substitute(Dictionary<string,string> subs = null) {
            //post-order transveral
            List<Node> newChildren = new List<Node>();
            for(int i=0; i<this.children.Count; i++) {
                newChildren.Add(this.children[i].Substitute(subs));
            }

            // replace variable name if needed
            string newName;
            if(subs!=null && subs.ContainsKey(this.functionName)) {
                newName = subs[this.functionName];
            } else {
                newName = this.functionName;
            }

            // apply the children to the parent
            Node newParent = new Node(newName, this.ns);
            foreach (var child in newChildren) {
                newParent.Apply(child);
            }
            return newParent;
        }

        public Node DeepCopy() {
            return this.Substitute();
        }





        // ----- S-Expression conversion ----------------------
        // convert an s-expression tree (names only, no types) to a node tree (typed)
        /*
                public static Node NodeFromSExpression(string sExpression, Namespace containerNS = null) {
                    containerNS = containerNS ?? Namespace.global;

                    // Parse S-Expression
                    STree sTree = new STree(sExpression);
                    if (sTree == null) return null;

                    // if top-level variable is known, and inputs are missing (partial application), add the inputs
                    if(containerNS.ContainsVariable(sTree.value)) {
                        int numberOfExpectedInputs = containerNS.VariableLookup(sTree.value).typeTree.GetNumberOfInputs();
                        int numberOfActualInputs = sTree.children.Count;
                        int idx = 0;
                        while(numberOfActualInputs!=numberOfExpectedInputs) {
                            string newVar = "x" + idx;
                            if(!sTree.Contains(newVar)) {
                                sTree.children.Add(STree.MakePrimitiveTree(newVar));
                                numberOfActualInputs++;
                            }
                            idx++;
                        }
                    }


                    // Perform type inference
                    Dictionary<string, TypeTree> variableTypes = new Dictionary<string, TypeTree>();
                    Dictionary<STree, TypeTree> expressionTypes = new Dictionary<STree, TypeTree>();
                    List<string> introducedVars = new List<string>(); // Dictionary<string,bool> would be faster, I guess
                    List<string> introducedTypeVars = new List<string>(); // Dictionary<string,bool> would be faster, I guess
                    bool success = TypeInference(sTree, containerNS, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                    if (!success) return null;


                    // make type variables more pretty

                    int charIdx = 97;
                    Dictionary<string, string> prettify = new Dictionary<string, string>();
                    foreach(var key in introducedTypeVars) {
                        string newName = ((char)charIdx).ToString();
                        if (!introducedTypeVars.Contains(newName)) {
                            prettify.Add(key, newName);
                        }
                        charIdx++;
                    }
                    foreach(var key in variableTypes.Keys) {
                        variableTypes[key].ReplaceString(prettify);
                    }


                    // Create a namespace will all the inferred types
                    // ns is where we can lookup variables from; expressionNS is the local expression NS where variables live
                    Namespace expressionNS = new Namespace(containerNS);
                    foreach (var var in introducedVars) {
                        // Can't just do expressionNS.AddVariable here; that erases the particular type of evaluation constant to use
                        // I'm re-adding certain variables here (like AND, true); not idea but it works... $TODO
                        expressionNS.AddVariable(var, variableTypes[var]);
                    }

                    // Use STree and namespace to create expression node
                    Node node = ExpressionFromSTree(sTree, expressionNS);

                    return node;
                }

                // given an S-Tree and a namespace containing the types of all the variables in the S-Tree, make an expression
                public static Node ExpressionFromSTree(STree sTree, Namespace expressionNS) {
                    Node parentNode = new Node(sTree.value, expressionNS);
                    foreach (var child in sTree.children) {
                        Node childNode = ExpressionFromSTree(child, expressionNS);
                        parentNode.Apply(childNode);
                    }
                    return parentNode;
                }


                // Given a parsed S-Expression, perform type inference and assign a type to every variable in the expression
                public static bool TypeInference(STree sTree, Namespace ns, Dictionary<string, TypeTree> variableTypes = null, Dictionary<STree, TypeTree> expressionTypes = null, List<string> introducedVars = null,  List<string> introducedTypeVars = null) {
                    variableTypes = variableTypes ?? new Dictionary<string, TypeTree>();            
                    expressionTypes = expressionTypes ?? new Dictionary<STree, TypeTree>();
                    introducedVars = introducedVars ?? new List<string>();
                    introducedTypeVars = introducedTypeVars ?? new List<string>();

                    // Do a post-order transversal (children first)
                    bool success = true;
                    foreach (var child in sTree.children) {
                        success = TypeInference(child, ns, variableTypes, expressionTypes, introducedVars, introducedTypeVars);
                        if (!success) return false;
                    }

                    // create a type variable (e.g. "e12") for the expression
                    // add this to introducedTypeVars so we don't confuse it with any other type variables called "e12" we might bring in
                    string eTypeVar = "e" + expressionTypes.Count;
                    expressionTypes.Add(sTree, new TypeTree(eTypeVar));
                    introducedTypeVars.Add(eTypeVar);

                    // get variable type format from types of child expressions
                    // 𝐸(𝑐_1) → 𝐸(𝑐_2) → … → 𝐸(𝑐_𝑛) → 𝐸(𝑝 𝑐_1 𝑐_2 … 𝑐_𝑛)
                    // where 𝐸(𝑝 𝑐_1 𝑐_2 … 𝑐_𝑛) = eTypeVar
                    List<TypeTree> treeList = new List<TypeTree>();
                    foreach (var child in sTree.children) {
                        treeList.Add(expressionTypes[child]);
                    }
                    treeList.Add(expressionTypes[sTree]); //output type
                    TypeTree t1 = TypeTree.TypeTreeFromTreeList(treeList);


                    // get variable name
                    string v = sTree.value;

                    // if it's a previously unseen variable symbol, and it doesn't exist in the namespace,
                    // all we know about it is the stuff we can infered from the child expression types
                    if (!variableTypes.ContainsKey(v) && !ns.variableLookup.ContainsKey(v)) {
                        variableTypes.Add(v, t1);
                        introducedVars.Add(v);                
                        return true; // nothing else meaningful we can do, so just return
                        // t1 == t2 here, so unify(t1,t2) is meaningless
                    }

                    // if it's a previously unseen variable symbol, but it exists in the namespace, look it up
                    if (!variableTypes.ContainsKey(v) && ns.variableLookup.ContainsKey(v)) {
                        TypeTree temp = ns.variableLookup[sTree.value].typeTree;
                        temp = temp.AddPrime(introducedTypeVars);
                        introducedTypeVars.AddRange(temp.GetTypeVariables());
                        variableTypes.Add(v, temp);
                    } // don't "else"! we WANT the next "if" to happen afterward! Stuff in the namespace may have type variables!!

                    // if it's a previously seen variable, get its type, and unify against t1 (child expression type info)
                    // update variable type and child expression types, using substitutions from unify()
                    if (variableTypes.ContainsKey(v)) {
                        TypeTree t2 = variableTypes[v];
                        Dictionary<string, TypeTree> subs = TypeTree.UnifyAndSolve(t1, t2);
                        if (subs == null) return false;
                        variableTypes[v] = t2.Substitute(subs);
                        expressionTypes[sTree] = expressionTypes[sTree].Substitute(subs);
                        foreach (var child in sTree.children) {
                            variableTypes[child.value] = variableTypes[child.value].Substitute(subs);
                            expressionTypes[child] = expressionTypes[child].Substitute(subs);
                        }
                        //$TODO: do I need to reach down more than one level here?
                    }

                    return true;
                }



        */





/*
        public Node SubstituteNode(Dictionary<string, Node> subs, Namespace containerNS) {
            Node parentNode;
            if (subs.ContainsKey(this.functionName)) {
                parentNode = new Node(subs[this.functionName].functionName, containerNS);
            } else {
                parentNode = new Node(this.functionName, containerNS);
            }

            foreach (var child in this.children) {
                Node childNode = child.SubstituteNode(subs, containerNS);
                parentNode.Apply(childNode);
            }
            return parentNode;
        }

        // ----- Printing to String -------------------------------------
        public override string ToString() {
            if (this.IsLeaf()) return this.functionName;

            string childrenString = "";
            foreach (var child in this.children) {
                childrenString += " " + child;
            }
            return "(" + this.functionName + childrenString + ")";
        }
    }
}
*/
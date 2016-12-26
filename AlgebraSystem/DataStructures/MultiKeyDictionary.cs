using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraSystem {
    class MultiKeyDictionary<TKey,TValue> where TValue : class {
        private List<Tuple<TKey, TValue>> dictionary;

        public MultiKeyDictionary() { 
            this.dictionary = new List<Tuple<TKey, TValue>>();
        }

        public void Add(TKey k, TValue v) {
            this.dictionary.Add(new Tuple<TKey, TValue>(k, v));
        }

        public bool ContainsKey(TKey k) {
            foreach(var kvp in this.dictionary) {
                if (k.Equals(kvp.Item1)) return true;
            }
            return false;
        }

        public TValue GetFirst(TKey k) {
            foreach (var kvp in this.dictionary) {
                if (k.Equals(kvp.Item1)) return kvp.Item2;
            }
            return null;
        }

        public bool DeleteFirst(TKey k) {
            for (int i = 0; i<this.dictionary.Count; i++) {
                if (k.Equals(this.dictionary[i].Item1)) {
                    this.dictionary.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public TValue GetFirstAndDelete(TKey k) {
            for (int i = 0; i < this.dictionary.Count; i++) {
                if (k.Equals(this.dictionary[i].Item1)) {
                    TValue v = this.dictionary[i].Item2;
                    this.dictionary.RemoveAt(i);
                    return v;
                }
            }
            return null;
        }

        public TValue this[TKey k] {
            get { return this.GetFirst(k); }
        }

    }
}

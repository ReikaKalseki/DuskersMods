using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReikaKalseki.DIDrones {
	
	public class CustomCommandDefinition : CommandDefinition {

		public readonly Regex[] argumentExpectations;

		public int minimumArguments { get {  return argumentExpectations.Length; } }

		public CustomCommandDefinition(string id, string desc, string sampleArgs, params Regex[] args) : base(id, desc, id+" "+sampleArgs) {
			argumentExpectations = args;
		}

		public bool validateArgument(string arg, int i) {
			return argumentExpectations[i] == null || argumentExpectations[i].IsMatch(arg);
		}

		public string validateArguments(List<string> args) {
			List<string> check = new List<string>(args);
			if (check.Count >= 1 && check[0] != null && new Regex("^[0-9]+$").IsMatch(check[0])) { //trim first arg if is drone ID
				check.RemoveAt(0);
			}
			if (check.Count < minimumArguments)
				return "Too few arguments";
			for (int i = 0; i < argumentExpectations.Length; i++) {
				if (!validateArgument(check[i], i))
					return string.Format("Invalid parameter #{0}: '{1}'", i, check[i]);
			}
			return null;
		}

	}
}

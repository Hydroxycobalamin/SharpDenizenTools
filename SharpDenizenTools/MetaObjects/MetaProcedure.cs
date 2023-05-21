using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using SharpDenizenTools.MetaHandlers;

namespace SharpDenizenTools.MetaObjects {
    /// <summary>A documented procedure script.</summary>
    public class MetaProcedure : MetaObject {
        /// <summary><see cref="MetaObject.Type"/></summary>
        public override MetaType Type => MetaDocs.META_TYPE_PROCEDURE;

        /// <summary><see cref="MetaObject.Name"/></summary>
        public override string Name => FullName;

        /// <summary><see cref="MetaObject.AddTo(MetaDocs)"/></summary>
        public override void AddTo(MetaDocs docs) {
            FullName = $"{MechObject}.{MechName}";
            NameForms = new string[] { FullName.ToLowerFast(), MechName.ToLowerFast() };
            HasMultipleNames = true;
            docs.Procedures.Add(CleanName, this);
        }

        /// <summary><see cref="MetaObject.MultiNames"/></summary>
        public override IEnumerable<string> MultiNames => NameForms;

        /// <summary>Both forms of the mech name (the full name, and the partial name).</summary>
        public string[] NameForms = Array.Empty<string>();

        /// <summary>The full procedure name (Object.Name).</summary>
        public string FullName;

        /// <summary>The object the procedure applies to.</summary>
        public string MechObject;

        /// <summary>The name of the procedure.</summary>
        public string MechName;

        /// <summary>The input type.</summary>
        public string Input;

        /// <summary>The long-form description.</summary>
        public string Description;

        /// <summary>Manual examples of this tag. One full script per entry.</summary>
        public List<string> Examples = new();

        /// <summary><see cref="MetaObject.ApplyValue(MetaDocs, string, string)"/></summary>
        public override bool ApplyValue(MetaDocs docs, string key, string value) {
            switch (key) {
                case "object":
                    MechObject = value;
                    return true;
                case "name":
                    MechName = value;
                    return true;
                case "input":
                    Input = value;
                    return true;
                case "description":
                    Description = value;
                    return true;
                case "example":
                    Examples.Add(value);
                    return true;
                default:
                    return base.ApplyValue(docs, key, value);
            }
        }

        /// <summary><see cref="MetaObject.PostCheck(MetaDocs)"/></summary>
        public override void PostCheck(MetaDocs docs) {
            PostCheckSynonyms(docs, docs.Procedures);
            Require(docs, MechObject, MechName, Input, Description);
            PostCheckLinkableText(docs, Description);
        }

        /// <summary><see cref="MetaObject.BuildSearchables"/></summary>
        public override void BuildSearchables() {
            base.BuildSearchables();
            SearchHelper.PerfectMatches.Add(MechName);
            SearchHelper.Strongs.Add(MechObject);
            SearchHelper.Decents.Add(Input);
            SearchHelper.Decents.Add(Description);
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using SharpDenizenTools.MetaHandlers;

namespace SharpDenizenTools.MetaObjects {
    /// <summary>A documented procedure script.</summary>
    public class MetaTask : MetaObject {
        /// <summary><see cref="MetaObject.Type"/></summary>
        public override MetaType Type => MetaDocs.META_TYPE_TASK;

        /// <summary><see cref="MetaObject.Name"/></summary>
        public override string Name => FullName;

        /// <summary><see cref="MetaObject.AddTo(MetaDocs)"/></summary>
        public override void AddTo(MetaDocs docs) {
            FullName = $"{MechName}";
            NameForms = new string[] { FullName.ToLowerFast(), MechName.ToLowerFast() };
            HasMultipleNames = true;
            docs.Tasks.Add(CleanName, this);
        }

        /// <summary><see cref="MetaObject.MultiNames"/></summary>
        public override IEnumerable<string> MultiNames => NameForms;

        /// <summary>Both forms of the mech name (the full name, and the partial name).</summary>
        public string[] NameForms = Array.Empty<string>();

        /// <summary>Whether the task MUST be injected.</summary>
        public bool MustInjected = false;

        /// <summary>The full procedure name (Object.Name).</summary>
        public string FullName;

        /// <summary>The name of the procedure.</summary>
        public string MechName;

        /// <summary>The input type.</summary>
        public string Input;

        /// <summary>The long-form description.</summary>
        public string Description;

        /// <summary>Sample usages.</summary>
        public List<string> Usages = new();

        /// <summary><see cref="MetaObject.ApplyValue(MetaDocs, string, string)"/></summary>
        public override bool ApplyValue(MetaDocs docs, string key, string value) {
            switch (key) {
                case "name":
                    MechName = value;
                    return true;
                case "input":
                    Input = value;
                    return true;
                case "description":
                    Description = value;
                    return true;
                case "usage":
                    Usages.Add(value);
                    return true;
                case "MustInjected":
                    MustInjected = value.Trim().ToLowerFast() == "true";
                    return true;
                default:
                    return base.ApplyValue(docs, key, value);
            }
        }

        /// <summary><see cref="MetaObject.PostCheck(MetaDocs)"/></summary>
        public override void PostCheck(MetaDocs docs) {
            PostCheckSynonyms(docs, docs.Tasks);
            Require(docs, MechName, Input, Description);
            PostCheckLinkableText(docs, Description);
        }

        /// <summary><see cref="MetaObject.BuildSearchables"/></summary>
        public override void BuildSearchables() {
            base.BuildSearchables();
            SearchHelper.PerfectMatches.Add(MechName);
            SearchHelper.Decents.Add(Input);
            SearchHelper.Backups.AddRange(Usages);
            SearchHelper.Decents.Add(Description);
        }
    }
}

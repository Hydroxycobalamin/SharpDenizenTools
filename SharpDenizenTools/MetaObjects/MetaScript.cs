using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SharpDenizenTools.MetaHandlers;
using FreneticUtilities.FreneticExtensions;

namespace SharpDenizenTools.MetaObjects {
    /// <summary>A script documentation.</summary>
    public class MetaScript : MetaObject {
        /// <summary><see cref="MetaObject.Type"/></summary>
        public override MetaType Type => MetaDocs.META_TYPE_SCRIPT;

        /// <summary><see cref="MetaObject.Name"/></summary>
        public override string Name => ScriptName;

        /// <summary><see cref="MetaObject.AddTo(MetaDocs)"/></summary>
        public override void AddTo(MetaDocs docs) {
            docs.Scripts.Add(CleanName, this);
        }

        /// <summary>The name of the scripts.</summary>
        public string ScriptName;

        /// <summary>The long-form description.</summary>
        public string Description;

        /// <summary>The download link.</summary>
        public string Download;

        /// <summary><see cref="MetaObject.ApplyValue(MetaDocs, string, string)"/></summary>
        public override bool ApplyValue(MetaDocs docs, string key, string value) {
            switch (key) {
                case "name":
                    ScriptName = value;
                    return true;
                case "description":
                    Description = value;
                    return true;
                case "download":
                    Download = value;
                    return true;
                default:
                    return base.ApplyValue(docs, key, value);
            }
        }

        /// <summary><see cref="MetaObject.PostCheck(MetaDocs)"/></summary>
        public override void PostCheck(MetaDocs docs) {
            PostCheckSynonyms(docs, docs.Scripts);
            Require(docs, ScriptName, Description);
            PostCheckLinkableText(docs, Description);
        }

        /// <summary><see cref="MetaObject.BuildSearchables"/></summary>
        public override void BuildSearchables() {
            base.BuildSearchables();
            SearchHelper.Decents.Add(Description);
        }
    }
}

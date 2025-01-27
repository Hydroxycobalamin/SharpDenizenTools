﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using SharpDenizenTools.MetaHandlers;

namespace SharpDenizenTools.MetaObjects
{
    /// <summary>A documented type of object.</summary>
    public class MetaObjectType : MetaObject
    {
        /// <summary><see cref="MetaObject.Type"/></summary>
        public override MetaType Type => MetaDocs.META_TYPE_OBJECT;

        /// <summary><see cref="MetaObject.Name"/></summary>
        public override string Name => TypeName;

        /// <summary><see cref="MetaObject.CleanName"/></summary>
        public override string CleanName => TypeName.ToLowerFast();

        /// <summary><see cref="MetaObject.AddTo(MetaDocs)"/></summary>
        public override void AddTo(MetaDocs docs)
        {
            docs.ObjectTypes.Add(CleanName, this);
            if (CleanName == "objecttag")
            {
                docs.ObjectTagType = this;
            }
            else if (CleanName == "elementtag")
            {
                docs.ElementTagType = this;
            }
        }

        /// <summary>The name of the object type.</summary>
        public string TypeName;

        /// <summary>The object identity prefix for this type.</summary>
        public string Prefix;

        /// <summary>The name of the base type.</summary>
        public string BaseTypeName;

        /// <summary>The base type.</summary>
        public MetaObjectType BaseType;

        /// <summary>A human-readable explanation of the identity format of the tag.</summary>
        public string Format;

        /// <summary>A human-readable description of the object type.</summary>
        public string Description;

        /// <summary>The names of other types or pseudo-types implemented by this type.</summary>
        public string[] ImplementsNames = Array.Empty<string>();

        /// <summary>All tags available directly to this base (not counting base/implements).</summary>
        public Dictionary<string, MetaTag> SubTags = new();

        /// <summary>Other types or pseudo-types implemented by this type.</summary>
        public MetaObjectType[] Implements = Array.Empty<MetaObjectType>();

        /// <summary>Other types or pseudo-types that extend this type.</summary>
        public List<MetaObjectType> ExtendedBy = new();

        /// <summary>The tag base component for generated examples, like 'player' for PlayerTag.</summary>
        public string GeneratedExampleTagBase;

        /// <summary>The object string for generated adjust examples, if mismatched from the tagbase, like '&lt;player&gt;' for PlayerTag.</summary>
        public string GeneratedExampleAdjust;

        /// <summary>A set of example text blocks for usage with tag return example generation.</summary>
        public List<string> GeneratedReturnUsageExample = new();

        /// <summary>Set of randomly selectable example values of this object type.</summary>
        public string[] ExampleValues = Array.Empty<string>();

        /// <summary>Information about matchable options for this type.</summary>
        public string Matchable;

        /// <summary><see cref="MetaObject.ApplyValue(MetaDocs, string, string)"/></summary>
        public override bool ApplyValue(MetaDocs docs, string key, string value)
        {
            switch (key)
            {
                case "name":
                    TypeName = value;
                    return true;
                case "prefix":
                    Prefix = value.ToLowerFast();
                    return true;
                case "base":
                    BaseTypeName = value;
                    return true;
                case "format":
                    Format = value;
                    return true;
                case "description":
                    Description = value;
                    return true;
                case "implements":
                    ImplementsNames = value.Replace(" ", "").SplitFast(',');
                    return true;
                case "exampletagbase":
                    GeneratedExampleTagBase = value;
                    if (GeneratedExampleAdjust is null)
                    {
                        GeneratedExampleAdjust = $"<{GeneratedExampleTagBase}>";
                    }
                    return true;
                case "exampleadjustobject":
                    GeneratedExampleAdjust = value;
                    return true;
                case "examplevalues":
                    ExampleValues = value.Replace(" ", "").SplitFast(',');
                    return true;
                case "exampleforreturns":
                    GeneratedReturnUsageExample.Add(value);
                    return true;
                case "matchable":
                    Matchable = value;
                    return true;
                default:
                    return base.ApplyValue(docs, key, value);
            }
        }

        /// <summary><see cref="MetaObject.PostCheck(MetaDocs)"/></summary>
        public override void PostCheck(MetaDocs docs)
        {
            Require(docs, TypeName, Prefix, BaseTypeName, Format, Description);
            if (BaseTypeName.ToLowerFast() != "none")
            {
                BaseType = docs.ObjectTypes.GetValueOrDefault(BaseTypeName.ToLowerFast());
                if (BaseType == null)
                {
                    docs.LoadErrors.Add($"Object type name '{TypeName}' specifies basetype '{BaseType}' which is invalid.");
                }
                else
                {
                    BaseType.ExtendedBy.Add(this);
                }
            }
            Implements = new MetaObjectType[ImplementsNames.Length];
            for (int i = 0; i < Implements.Length; i++)
            {
                Implements[i] = docs.ObjectTypes.GetValueOrDefault(ImplementsNames[i].ToLowerFast());
                if (Implements[i] == null)
                {
                    docs.LoadErrors.Add($"Object type name '{TypeName}' specifies implement type '{Implements[i]}' which is invalid.");
                }
                else
                {
                    Implements[i].ExtendedBy.Add(this);
                }
            }
            PostCheckSynonyms(docs, docs.ObjectTypes);
            if (!TypeName.EndsWith("Tag") && !TypeName.EndsWith("Object") && (Prefix.ToLowerFast() != "none" || BaseTypeName.ToLowerFast() != "none"))
            {
                docs.LoadErrors.Add($"Object type name '{TypeName}' has unrecognized format.");
            }
            if (Prefix != "none" && docs.ObjectTypes.Values.Any(t => t != this && t.Prefix == Prefix))
            {
                docs.LoadErrors.Add($"Object type name '{TypeName}' uses prefix '{Prefix}' which is also used by another object type.");
            }
            string lowName = CleanName;
            foreach (MetaTag tag in docs.Tags.Values.Where(t => t.BeforeDot.ToLowerFast() == lowName))
            {
                SubTags.Add(tag.AfterDotCleaned, tag);
            }
            MetaObjectType subType = BaseType;
            for (int i = 0; i < 20; i++)
            {
                if (subType == null)
                {
                    break;
                }
                subType = docs.ObjectTypes.GetValueOrDefault(subType.BaseTypeName.ToLowerFast());
            }
            if (subType != null)
            {
                docs.LoadErrors.Add($"Object type name '{subType.Name}' has base type '{subType.BaseTypeName}' which appears to be a recursive loop.");
            }
            PostCheckLinkableText(docs, Description);
        }

        /// <summary><see cref="MetaObject.BuildSearchables"/></summary>
        public override void BuildSearchables()
        {
            base.BuildSearchables();
            SearchHelper.Strongs.Add(Prefix);
            SearchHelper.Decents.Add(Description);
            SearchHelper.Backups.Add(Format);
        }
    }
}

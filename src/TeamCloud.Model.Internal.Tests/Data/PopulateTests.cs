/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TeamCloud.Model.Internal.Data
{
    public class PopulateTests
    {
        public class InternalModelA : IPopulate<ExternalModelA>
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public double DoubleProp { get; set; }
            public string StringPropExtra { get; set; }
            public int IntPropExtra { get; set; }
            public double DoublePropExtra { get; set; }
            public IDictionary<string, string> StringDict { get; set; }
            public InternalModelB ModelB { get; set; }
            public IList<InternalModelB> ModelBList { get; set; }
            public static InternalModelA Create(int intProp, double doubleProp)
                => new InternalModelA
                {
                    StringProp = "A Internal String",
                    IntProp = intProp,
                    DoubleProp = doubleProp,
                    StringPropExtra = "Another String",
                    IntPropExtra = intProp,
                    DoublePropExtra = doubleProp,
                    StringDict = GetDictionary("InternalModelA"),
                    ModelB = InternalModelB.Create(intProp, doubleProp),
                    ModelBList = new List<InternalModelB> {
                        InternalModelB.Create(intProp, doubleProp),
                        InternalModelB.Create(intProp, doubleProp),
                        InternalModelB.Create(intProp, doubleProp)
                    }
                };
        }

        public class InternalModelB : IPopulate<ExternalModelB>
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public double DoubleProp { get; set; }
            public string StringPropExtra { get; set; }
            public int IntPropExtra { get; set; }
            public double DoublePropExtra { get; set; }
            public static InternalModelB Create(int intProp, double doubleProp)
                => new InternalModelB
                {
                    StringProp = "A Internal String",
                    IntProp = intProp,
                    DoubleProp = doubleProp,
                    StringPropExtra = "Another String",
                    IntPropExtra = intProp,
                    DoublePropExtra = doubleProp,
                };
        }

        public class ExternalModelA
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public double DoubleProp { get; set; }
            public IDictionary<string, string> StringDict { get; set; }
            public ExternalModelB ModelB { get; set; }
            public IList<ExternalModelB> ModelBList { get; set; }
            public static ExternalModelA Create(int intProp, double doubleProp)
                => new ExternalModelA
                {
                    StringProp = "A External String",
                    IntProp = intProp,
                    DoubleProp = doubleProp,
                    StringDict = GetDictionary("ExternalModelA"),
                    ModelB = ExternalModelB.Create(intProp, doubleProp),
                    ModelBList = new List<ExternalModelB> {
                        ExternalModelB.Create(intProp, doubleProp),
                        ExternalModelB.Create(intProp, doubleProp),
                        ExternalModelB.Create(intProp, doubleProp)
                    }
                };
        }

        public class ExternalModelB
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public double DoubleProp { get; set; }
            public static ExternalModelB Create(int intProp, double doubleProp)
                => new ExternalModelB
                {
                    StringProp = "A External String",
                    IntProp = intProp,
                    DoubleProp = doubleProp
                };
        }

        static Dictionary<string, string> GetDictionary(string type)
            => new Dictionary<string, string> {
                { $"{type}DictOne", $"{type}DictOneValue" },
                { $"{type}DictTwo", $"{type}DictTwoValue" }
            };


        [Fact]
        public void PopulateExternalModel()
        {
            var random = new Random();

            var intProp = random.Next();
            var doubleProp = random.NextDouble();

            var source = InternalModelA.Create(intProp, doubleProp);
            var target = source.PopulateExternalModel<InternalModelA, ExternalModelA>();

            Assert.Equal(target.StringProp, source.StringProp);
            Assert.Equal(target.IntProp, source.IntProp);
            Assert.Equal(target.DoubleProp, source.DoubleProp);
            Assert.Equal(target.StringDict, source.StringDict);

            Assert.Equal(target.ModelB.StringProp, source.ModelB.StringProp);
            Assert.Equal(target.ModelB.IntProp, source.ModelB.IntProp);
            Assert.Equal(target.ModelB.DoubleProp, source.ModelB.DoubleProp);

            foreach (var model in source.ModelBList)
                Assert.Contains(target.ModelBList, m => m.StringProp == model.StringProp
                                                     && m.IntProp == model.IntProp
                                                     && m.DoubleProp == model.DoubleProp);
        }

        [Fact]
        public void PopulateFromExternalModel()
        {
            var random = new Random();

            var sourceIntProp = random.Next();
            var sourceDoubleProp = random.NextDouble();

            var targetIntProp = random.Next();
            var targetDoubleProp = random.NextDouble();

            var source = ExternalModelA.Create(sourceIntProp, sourceDoubleProp);
            var target = InternalModelA.Create(targetIntProp, targetDoubleProp);

            target.PopulateFromExternalModel(source);

            Assert.Equal(target.StringProp, source.StringProp);
            Assert.Equal(target.IntProp, source.IntProp);
            Assert.Equal(target.DoubleProp, source.DoubleProp);
            Assert.Equal(target.StringPropExtra, "Another String");
            Assert.Equal(target.IntPropExtra, targetIntProp);
            Assert.Equal(target.DoublePropExtra, targetDoubleProp);

            Assert.Equal(target.StringDict, source.StringDict);

            Assert.Equal(target.ModelB.StringProp, source.ModelB.StringProp);
            Assert.Equal(target.ModelB.IntProp, source.ModelB.IntProp);
            Assert.Equal(target.ModelB.DoubleProp, source.ModelB.DoubleProp);

            Assert.Equal(target.ModelB.StringPropExtra, "Another String");
            Assert.Equal(target.ModelB.IntPropExtra, targetIntProp);
            Assert.Equal(target.ModelB.DoublePropExtra, targetDoubleProp);

            foreach (var model in source.ModelBList)
            {
                Assert.Contains(target.ModelBList, m => m.StringProp == model.StringProp
                                                     && m.IntProp == model.IntProp
                                                     && m.DoubleProp == model.DoubleProp);
                //  && m.StringPropExtra == "Another String"
                //  && m.IntPropExtra == targetIntProp
                //  && m.DoublePropExtra == targetDoubleProp);
            }
        }
    }
}

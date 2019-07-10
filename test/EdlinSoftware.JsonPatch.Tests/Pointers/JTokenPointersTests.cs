using System;
using EdlinSoftware.JsonPatch.Pointers;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests.Pointers
{
    public class JTokenPointersTests
    {
        [Fact]
        public void RootObjectPointer()
        {
            JObject jObject = JObject.Parse("{}");

            var pointer = JTokenPointer.Get(jObject, "");

            pointer.ShouldBeOfType<JRootPointer>();
        }

        [Fact]
        public void RootArrayPointer()
        {
            JArray jArray = JArray.Parse("[]");

            var pointer = JTokenPointer.Get(jArray, "");

            pointer.ShouldBeOfType<JRootPointer>();
        }

        [Fact]
        public void RootObjectPropertyPointer()
        {
            JObject jObject = JObject.Parse("{}");

            var pointer = JTokenPointer.Get(jObject, "/var");

            var jObjectPointer = pointer.ShouldBeOfType<JObjectPointer>();

            var (jObj, pathPart) = jObjectPointer;

            jObject.ShouldBeSameAs(jObj);
            pathPart.ShouldBe("var");
        }

        [Fact]
        public void RootArrayItemPointer()
        {
            JArray jArray = JArray.Parse("[]");

            var pointer = JTokenPointer.Get(jArray, "/0");

            var jArrayPointer = pointer.ShouldBeOfType<JArrayPointer>();

            var (jArr, pathPart) = jArrayPointer;

            jArray.ShouldBeSameAs(jArr);
            pathPart.ShouldBe("0");
        }

        [Fact]
        public void NestedObjectPropertyPointer()
        {
            JObject jObject = JObject.Parse("{ \"var\": {} }");

            var pointer = JTokenPointer.Get(jObject, "/var/boo");

            var jObjectPointer = pointer.ShouldBeOfType<JObjectPointer>();

            var (jObj, pathPart) = jObjectPointer;

            jObject.GetValue("var").ShouldBeSameAs(jObj);
            pathPart.ShouldBe("boo");
        }

        [Fact]
        public void PointerToObjectInsideArray()
        {
            JArray jArray = JArray.Parse("[ {}, {} ]");

            var pointer = JTokenPointer.Get(jArray, "/1/bar");

            var jObjectPointer = pointer.ShouldBeOfType<JObjectPointer>();

            var (jObj, pathPart) = jObjectPointer;

            jArray[1].ShouldBeSameAs(jObj);
            pathPart.ShouldBe("bar");
        }

        [Fact]
        public void PointerToArrayInsideObject()
        {
            JObject jObject = JObject.Parse("{ \"var\": [] }");

            var pointer = JTokenPointer.Get(jObject, "/var/-");

            var jArrayPointer = pointer.ShouldBeOfType<JArrayPointer>();

            var (jArr, pathPart) = jArrayPointer;

            jObject.GetValue("var").ShouldBeSameAs(jArr);
            pathPart.ShouldBe("-");
        }

        [Fact]
        public void PointerToLastObjectInsideArray()
        {
            JArray jArray = JArray.Parse("[ {}, {} ]");

            var pointer = JTokenPointer.Get(jArray, "/-/bar");

            var jObjectPointer = pointer.ShouldBeOfType<JObjectPointer>();

            var (jObj, pathPart) = jObjectPointer;

            jArray[1].ShouldBeSameAs(jObj);
            pathPart.ShouldBe("bar");
        }

        [Fact]
        public void PointerToAbsentObjectProperty()
        {
            JObject jObject = JObject.Parse("{ \"var\": [] }");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jObject, "/boo/var");
            });

            exception.Message.ShouldContain("'/boo'");
        }

        [Fact]
        public void PointerToAbsentArrayIndex_InTheMiddleOfPath()
        {
            JArray jArray = JArray.Parse("[ {}, {} ]");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jArray, "/4/var");
            });

            exception.Message.ShouldContain("''");
            exception.Message.ShouldContain("'4'");
        }

        [Fact]
        public void PointerToAbsentArrayIndex_InTheEndOfPath()
        {
            JArray jArray = JArray.Parse("[ {}, {} ]");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jArray, "/4");
            });

            exception.Message.ShouldContain("''");
            exception.Message.ShouldContain("'4'");
        }

        [Fact]
        public void PointerToPropertyInArray()
        {
            JArray jArray = JArray.Parse("[ {}, {} ]");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jArray, "/var/bar");
            });

            exception.Message.ShouldContain("''");
            exception.Message.ShouldContain("'var'");
        }

        [Fact]
        public void PointerThroughPrimitiveType()
        {
            JObject jObject = JObject.Parse("{ \"var\": 3 }");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jObject, "/var/bar/foo");
            });

            exception.Message.ShouldContain("'/var'");
            exception.Message.ShouldContain("'Integer'");
        }

        [Fact]
        public void FinalTokenIsNeitherObjectNorArray()
        {
            JObject jObject = JObject.Parse("{ \"var\": 3 }");

            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                JTokenPointer.Get(jObject, "/var/bar");
            });

            exception.Message.ShouldContain("'/var/bar'");
        }

        [Fact]
        public void SpecialSymbolsInPaths()
        {
            JObject jObject = JObject.Parse("{ \"foo/bar~\": {} }");

            var pointer = JTokenPointer.Get(jObject, "/foo~1bar~0/7");

            var jObjectPointer = pointer.ShouldBeOfType<JObjectPointer>();

            var (jObj, pathPart) = jObjectPointer;

            jObject.GetValue("foo/bar~").ShouldBeSameAs(jObj);
            pathPart.ShouldBe("7");
        }
    }
}
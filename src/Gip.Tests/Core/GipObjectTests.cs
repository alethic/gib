using System.Collections.Generic;

using FluentAssertions;

using Gip.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gip.Tests.Core
{

    [TestClass]
    public class GipObjectTests
    {

        class TestObject : GipObject<ParentTestObject>
        {



        }

        class ParentTestObject : GipObject
        {

            readonly List<GipObject> children = new();

            public void Add(GipObject child)
            {
                child.Parent = this;
                children.Add(child);
            }
        
        }

        [TestMethod]
        public void SettingNameShouldRaisePropertyChangingEvent()
        {
            var b = false;
            var o = new TestObject();
            o.Event += (s, a) =>
            {
                if (a is GipPropertyChangingEventArgs args && args.Name == nameof(o.Name))
                    b = (string?)args.OldValue == "" && (string?)args.NewValue == "Bar" && o.Name == "";
            };
            o.Name = "Bar";
            b.Should().BeTrue();
            o.Name.Should().Be("Bar");
        }

        [TestMethod]
        public void SettingNameShouldRaisePropertyChangedEvent()
        {
            var b = false;
            var o = new TestObject();
            o.Event += (s, a) =>
            {
                if (a is GipPropertyChangedEventArgs args && args.Name == nameof(o.Name))
                    b = (string?)args.OldValue == "" && (string?)args.NewValue == "Bar" && o.Name == "Bar";
            };
            o.Name = "Bar";
            b.Should().BeTrue();
            o.Name.Should().Be("Bar");
        }

        [TestMethod]
        public void SettingParentShouldRaisePropertyChangingEvent()
        {
            var b = false;
            var p = new ParentTestObject();
            var o = new TestObject();
            o.Event += (s, a) =>
            {
                if (a is GipPropertyChangingEventArgs args && args.Name == nameof(o.Parent))
                    b = (GipObject?)args.OldValue == null && (GipObject?)args.NewValue == p && o.Parent == null;
            };
            o.Parent = p;
            b.Should().BeTrue();
            o.Parent.Should().Be(p);
        }

        [TestMethod]
        public void SettingParentShouldRaisePropertyChangedEvent()
        {
            var b = false;
            var p = new ParentTestObject();
            var o = new TestObject();
            o.Event += (s, a) =>
            {
                if (a is GipPropertyChangedEventArgs args && args.Name == nameof(o.Parent))
                    b = (GipObject?)args.OldValue == null && (GipObject?)args.NewValue == p && o.Parent == p;
            };
            o.Parent = p;
            b.Should().BeTrue();
            o.Parent.Should().Be(p);
        }

        [TestMethod]
        public void DefaultRootShouldBeSelf()
        {
            var o = new TestObject();
            o.root.Should().Be(o);
        }

        [TestMethod]
        public void SettingParentShouldReplaceRoot()
        {
            var p = new ParentTestObject();
            var o = new TestObject();
            p.Add(o);
            o.root.Should().Be(p);
        }

        [TestMethod]
        public void SettingParentShouldReplaceRootOfEntireGraph()
        {
            var p1 = new ParentTestObject();
            var p2 = new ParentTestObject();
            var o = new TestObject();
            p2.Add(o);
            o.root.Should().Be(p2);
            p1.Add(p2);
            p1.root.Should().Be(p1);
            p2.root.Should().Be(p1);
            o.root.Should().Be(p1);
        }

    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Frends.PostgreSQL.ExecuteQueryToFile.Attributes;
using Frends.PostgreSQL.ExecuteQueryToFile.Helpers;
using NUnit.Framework;

namespace Frends.PostgreSQL.ExecuteQueryToFile.Tests.HelpersTests;

[TestFixture]
internal class ValidationHandlerTests
{
    [Test]
    public void ShouldPassForValidObject()
    {
        Assert.DoesNotThrow(() => ValidationHandler.Run(new Root
        {
            Name = "a",
            Child = new Child { Title = "t" },
        }));
    }

    [Test]
    public void ShouldFailWhenNoObjectsProvided()
    {
        Assert.Throws<ValidationException>(() => ValidationHandler.Run());
        Assert.Throws<ValidationException>(() => ValidationHandler.Run(null));
        Assert.Throws<ValidationException>(() => ValidationHandler.Run(Array.Empty<object>()));
    }

    [Test]
    public void ShouldFailWhenAnyTopLevelObjectIsNull()
    {
        Assert.Throws<ValidationException>(() => ValidationHandler.Run(new Root { Name = "a" }, null));
    }

    [Test]
    public void ShouldFailForInvalidTopLevelProperty()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(new Root { Name = null }));
        Assert.That(ex.Message, Does.Contain("Name"));
    }

    [Test]
    public void ShouldValidateNestedObjectProperties()
    {
        var root = new Root
        {
            Name = "a",
            Child = new Child { Title = null },
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message, Does.Contain("Title"));
    }

    [Test]
    public void ShouldValidateDeeplyNestedObjects()
    {
        var root = new Root
        {
            Name = "a",
            Child = new Child
            {
                Title = "t",
                GrandChild = new Child
                {
                    Title = "g",
                    GrandChild = new Child(),
                },
            },
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message, Does.Contain("Title"));
    }

    [Test]
    public void ShouldValidateEveryCollectionItemNotOnlyFirst()
    {
        var root = new Root
        {
            Name = "a",
            Items = [new Child { Title = "ok" }, new Child { Title = "ok" }, new Child { Title = null }],
        };

        Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
    }

    [Test]
    public void ShouldReportErrorsFromAllItems()
    {
        var root = new Root
        {
            Name = null,
            Items = [new Child(), new Child()],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message.Split("Title").Length - 1, Is.EqualTo(2));
        Assert.That(ex.Message, Does.Contain("Name"));
    }

    [Test]
    public void ShouldFailForNullCollectionItems()
    {
        var root = new Root
        {
            Name = "a",
            Items = [new Child { Title = "t" }, null],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message, Does.Contain("Validated object can't be null!"));
    }

    [Test]
    public void ShouldReportEveryNullCollectionItem()
    {
        var root = new Root
        {
            Name = "a",
            Items = [null, new Child { Title = "t" }, null],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message.Split("can't be null").Length - 1, Is.EqualTo(2));
    }

    [Test]
    public void ShouldReportBothNullAndInvalidItems()
    {
        var root = new Root
        {
            Name = "a",
            Items = [null, new Child { Title = null }],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message, Does.Contain("can't be null"));
        Assert.That(ex.Message, Does.Contain("Title"));
    }

    [Test]
    public void ShouldAcceptNullPropertiesWithoutAttributes()
    {
        Assert.DoesNotThrow(() => ValidationHandler.Run(new Root
        {
            Name = "a",
            Child = null,
            Items = null,
        }));
    }

    [Test]
    public void ShouldAcceptEmptyCollections()
    {
        Assert.DoesNotThrow(() => ValidationHandler.Run(new Root
        {
            Name = "a",
            Items = [],
        }));
    }

    [Test]
    public void ShouldFailForNullItemsInNestedCollections()
    {
        var root = new Root
        {
            Name = "a",
            NestedLists = [[null]],
        };

        Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
    }

    [Test]
    public void ShouldValidateCollectionsPassedDirectly()
    {
        Assert.Throws<ValidationException>(() => ValidationHandler.Run(new List<Child> { new() }));
        Assert.Throws<ValidationException>(() => ValidationHandler.Run(new List<Child>
        {
            null,
            new() { Title = "t" },
        }));
        Assert.DoesNotThrow(() => ValidationHandler.Run(new List<Child> { new() { Title = "t" } }));
    }

    [Test]
    public void ShouldPrefixNullItemMessage()
    {
        var root = new Root
        {
            Name = "a",
            Items = [null],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message, Does.StartWith("Validation failed:"));
    }

    [Test]
    public void ShouldNotRecurseIntoObjectTypedProperties()
    {
        // Value holds an invalid object, but object-typed (dynamic) properties are treated as leaves.
        var root = new Root
        {
            Name = "a",
            Value = new Child(),
        };

        Assert.DoesNotThrow(() => ValidationHandler.Run(root));
    }

    [Test]
    public void ShouldHandleBasicTypesWithoutRecursing()
    {
        var root = new Root
        {
            Name = "a",
            Number = 5,
            NullableNumber = null,
            Date = DateTime.UtcNow,
            Offset = DateTimeOffset.UtcNow,
            Span = TimeSpan.FromSeconds(1),
            Id = Guid.NewGuid(),
            Bytes = [1, 2, 3],
            Kind = DayOfWeek.Monday,
            Amount = 1.5m,
            Value = DateTime.UtcNow,
        };

        Assert.DoesNotThrow(() => ValidationHandler.Run(root));
    }

    [Test]
    public void ShouldHandleCircularReferences()
    {
        var a = new Child { Title = "a" };
        var b = new Child
        {
            Title = "b",
            GrandChild = a,
        };
        a.GrandChild = b;

        Assert.DoesNotThrow(() => ValidationHandler.Run(a));
    }

    [Test]
    public void ShouldReportInvalidSharedInstanceOnlyOnce()
    {
        var shared = new Child();
        var root = new Root
        {
            Name = "a",
            Items = [shared, shared],
        };

        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));
        Assert.That(ex.Message.Split("Title").Length - 1, Is.EqualTo(1));
    }

    [Test]
    public void ShouldIgnoreIndexersAndWriteOnlyProperties()
    {
        Assert.DoesNotThrow(() => ValidationHandler.Run(new WithIndexer()));
    }

    [Test]
    public void ShouldValidateMultipleObjects()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            ValidationHandler.Run(new Root { Name = "a" }, new Child { Title = null }));
        Assert.That(ex.Message, Does.Contain("Title"));
    }

    [Test]
    public void ShouldWorkWithCustomValidationAttributesInNestedObjects()
    {
        var root = new Root
        {
            Name = "a",
            Child = new Child
            {
                Title = "t",
                Mode = "x",
                Extra = null,
            },
        };

        Assert.Throws<ValidationException>(() => ValidationHandler.Run(root));

        root.Child.Extra = "provided";
        Assert.DoesNotThrow(() => ValidationHandler.Run(root));
    }

    private class Root
    {
        [Required]
        public string Name { get; set; }

        public Child Child { get; set; }

        public List<Child> Items { get; set; }

        public List<List<Child>> NestedLists { get; set; }

        public object Value { get; set; }

        public int Number { get; set; }

        public int? NullableNumber { get; set; }

        public DateTime Date { get; set; }

        public DateTimeOffset Offset { get; set; }

        public TimeSpan Span { get; set; }

        public Guid Id { get; set; }

        public byte[] Bytes { get; set; }

        public DayOfWeek Kind { get; set; }

        public decimal Amount { get; set; }
    }

    private class Child
    {
        [Required]
        public string Title { get; set; }

        public Child GrandChild { get; set; }

        public string Mode { get; set; }

        [RequiredIf(nameof(Mode), "x")]
        public string Extra { get; set; }
    }

    private class WithIndexer
    {
        public string WriteOnly
        {
            set { }
        }

        public string this[int i] => "x";
    }
}

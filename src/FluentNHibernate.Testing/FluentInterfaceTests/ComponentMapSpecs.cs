﻿using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using NUnit.Framework;

namespace FluentNHibernate.Testing.FluentInterfaceTests
{
    [TestFixture]
    public class when_creating_the_mapping_for_a_component_using_ComponentMap : Specification
    {
        public override void establish_context()
        {
            component = new ComponentMap<Target>();
            component.Map(x => x.a_property);
            component.ReferencesAny(x => x.an_any).EntityIdentifierColumn("x").EntityTypeColumn("y").IdentityType(typeof(Target));
            component.HasMany(x => x.a_collection);
            component.Component(x => x.a_component);
            component.HasOne(x => x.a_one_to_one);
            component.References(x => x.a_reference);
        }

        public override void because()
        {
            mapping = (component as IExternalComponentMappingProvider).GetComponentMapping();
        }

        [Test]
        public void should_create_an_external_component_mapping()
        {
            mapping.ShouldBeOfType<ExternalComponentMapping>();
        }

        [Test]
        public void should_add_properties_to_the_properties_collection()
        {
            mapping.Properties.ShouldContain(x => x.Name == "a_property");
        }

        [Test]
        public void should_add_anys_to_the_anys_collection()
        {
            mapping.Anys.ShouldContain(x => x.Name == "an_any");
        }

        [Test]
        public void should_add_collections_to_the_collections_collection()
        {
            mapping.Collections.ShouldContain(x => x.Name == "a_collection");
        }

        [Test]
        public void should_add_components_to_the_components_collection()
        {
            mapping.Components.ShouldContain(x => x.Name == "a_component");
        }

        [Test]
        public void should_add_one_to_ones_to_the_one_to_ones_collection()
        {
            mapping.OneToOnes.ShouldContain(x => x.Name == "a_one_to_one");
        }

        [Test]
        public void should_add_references_to_the_references_collection()
        {
            mapping.References.ShouldContain(x => x.Name == "a_reference");
        }
        
        private ComponentMap<Target> component;
        private IComponentMapping mapping;

        private class Target
        {
            public string a_property { get; set; }
            public Target an_any { get; set; }
            public Target a_component { get; set; }
            public Target a_one_to_one { get; set; }
            public Target a_reference { get; set; }
            public IEnumerable<Target> a_collection { get; set; }
        }
    }

    [TestFixture]
    public class when_mapping_a_component_in_an_entity_without_defining_any_mappings_for_the_component : Specification
    {
        public override void establish_context()
        {
            classmap = new ClassMap<Target>();
            classmap.Component(x => x.Component);
        }

        public override void because()
        {
            mapping = (classmap as IMappingProvider).GetClassMapping()
                .Components.First();
        }

        [Test]
        public void should_create_a_reference_component_mapping()
        {
            mapping.ShouldBeOfType<ReferenceComponentMapping>();
        }

        [Test]
        public void should_store_the_property_in_the_reference_component_mapping()
        {
            (mapping as ReferenceComponentMapping).Member.Name.ShouldBeEqualIgnoringCase("Component");
        }

        private ClassMap<Target> classmap;
        private IComponentMapping mapping;

        private class Target
        {
            public Component Component { get; set;}
        }

        private class Component
        {
            
        }
    }

    [TestFixture]
    public class when_compiling_the_mappings_with_a_reference_component_and_a_related_external_component : Specification
    {
        public override void establish_context()
        {
            var component_map = new ComponentMap<Component>();
            component_map.Map(x => x.Property);

            var class_map = new ClassMap<Target>();
            class_map.Component(x => x.Component);

            persistence_model = new PersistenceModel();
            persistence_model.Add(class_map);
            persistence_model.Add(component_map);
        }

        public override void because()
        {
            mappings = persistence_model.BuildMappings();
            class_mapping = mappings.SelectMany(x => x.Classes).First();
        }

        [Test]
        public void should_merge_the_delegated_component_mapping_with_the_unassociated_component_mapping_from_the_component_map()
        {
            var component_mapping = class_mapping.Components.First();

            component_mapping.Member.Name.ShouldBeEqualIgnoringCase("Component");
            component_mapping.Properties.ShouldContain(x => x.Name == "Property");
        }

        private PersistenceModel persistence_model;
        private IEnumerable<HibernateMapping> mappings;
        private ClassMapping class_mapping;

        private class Target
        {
            public Component Component { get; set; }
        }

        private class Component
        {
            public string Property { get; set; }
        }
    }
}
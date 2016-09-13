﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Haumea.Components;
using Haumea.Collections;
using Haumea.Geometric;
using Haumea.Rendering;
using Haumea.UIForms;
using Haumea.Parsing;

using TagIdMap = Haumea.Collections.BiDictionary<int, string>;

namespace Haumea.Game
{
    public static class IntializeRawData
    {
        public static InitializedRawGameData Initialize(RawGameData data, ContentManager content)
        {
            var realmsTagId    = InitializeRealmTags(data.RawRealms);
            var provincesTagId = InitializeProvinceTags(data.RawProvinces);

            var events    = new EventController();
            var mapGraph  = InitializeMapGraph(data.RawConnectors, provincesTagId);
            var provinces = InitializeProvinces(data.RawProvinces, mapGraph);
            var realms    = new Realms();
            var units     = InitializeUnits(data.RawArmies, provincesTagId, realmsTagId, provinces, events);

            // TODO: I have realized these two (provinces & units) are so tangled into each other
            // ..... they should probably be merged into something like "mapView"
         
            WindowsTree windows = new WindowsTree();
            FormCreator ui = new FormCreator(content, windows);
            //ProvincesView provincesView = InitializeProvincesView(data.RawProvinces, provinces);
            //UnitsView unitsView = InitializeUnitsView(provinces, units, ui);
            //DebugTextView debugView = new DebugTextView(provinces, units, realmsTagId, provincesTagId);

            RenderInstruction[][] standardInstrs = new RenderInstruction[provinces.Boundaries.Length][];
            RenderInstruction[][] idleInstrs     = new RenderInstruction[provinces.Boundaries.Length][];

            for (int id = 0; id < provinces.Boundaries.Length; id++)
            {
                MultiPoly mpoly = provinces.Boundaries[id];

                Color c = data.RawProvinces[id].Color;
                if (data.RawProvinces[id].IsWater) c = Color.Blue;

                RenderInstruction[] mpolyInstrs = RenderInstruction.MultiPolygon(
                    mpoly, c, Color.Blue);

                standardInstrs[id] = mpolyInstrs;

                idleInstrs[id] = RenderInstruction.MultiPolygon(mpoly, c.Darken(), Color.Black);
            }

            var dialogManager = new DialogManager();
            var mapView = new MapView(provinces, units, standardInstrs, idleInstrs, dialogManager);
                
            IList<IModel> models = new List<IModel> {
                events, provinces, realms, units
            };
            
            IList<IView> views = new List<IView> {
                mapView, dialogManager
            };

            return new InitializedRawGameData(models, views, dialogManager);
        }

        private static BiDictionary<int, string> InitializeProvinceTags(IList<RawProvince> rProvinces)
        {
            TagIdMap tagIdMapping = new TagIdMap();

            for (int id = 0; id < rProvinces.Count; id++) {
                tagIdMapping.Add(id, rProvinces[id].Tag);
            }

            return tagIdMapping;            
        }

        private static BiDictionary<int, string> InitializeRealmTags(IList<RawRealm> rRealms)
        {
            TagIdMap tagIdMapping = new TagIdMap();

            for (int id = 0; id < rRealms.Count; id++)
            {
                tagIdMapping[id] = rRealms[id].Tag;
            }

            return tagIdMapping;
        }

        private static Provinces InitializeProvinces(IList<RawProvince> rProvinces, NodeGraph<int> graph)
        {
            MultiPoly[] boundaries = new MultiPoly[rProvinces.Count];
            ISet<int> waterProvinces = new HashSet<int>();

            for (int id = 0; id < rProvinces.Count; id++) {
                // TODO: Non-complex provinces should just be kept as a single polyon,
                // ..... not a shape.
                boundaries[id] = new MultiPoly(rProvinces[id].Polys.ToArray());
                if (rProvinces[id].IsWater)
                {
                    waterProvinces.Add(id);
                }
            }

            return new Provinces(boundaries, waterProvinces, graph);
        }

        private static NodeGraph<int> InitializeMapGraph(IList<RawConnector> rConns, TagIdMap provinceTagId)
        {
            IList<Connector<int>> conns = new List<Connector<int>>();

            foreach (RawConnector rconn in rConns)
            {
                int ID1 = provinceTagId[rconn.Tag1];
                int ID2 = provinceTagId[rconn.Tag2];
                conns.Add(new Connector<int>(ID1, ID2, rconn.Cost));
            }

            return new NodeGraph<int>(conns, true);
        }

        private static Units InitializeUnits(IList<RawArmy> rawArmies, TagIdMap provinceTagId, TagIdMap realmsTagId,
            Provinces provinces, EventController events)
        {
            Units units = new Units(provinces, events);

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID    = realmsTagId[rawArmy.Owner];
                int locationID = provinceTagId[rawArmy.Location];
                Units.Army army = new Units.Army(ownerID, locationID, rawArmy.NUnits);
                units.AddArmy(army);
            }

            return units;
        }
    }
        
    public struct InitializedRawGameData
    {
        public IList<IModel> Models { get; }
        public IList<IView> Views { get; }
        public DialogManager DialogManager { get; }

        public InitializedRawGameData(IList<IModel> models, IList<IView> views, DialogManager dialogManager)
        {
            Models = models;
            Views = views;
            DialogManager = dialogManager;
        }
    }
}


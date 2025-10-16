using System;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Crafting;
using UnityEngine;

namespace ResourceMonitor
{
    public class ResourceMonitor
    {
        public static readonly Vector3 LargeScale = new Vector3(2.3f, 2.3f, 1f);
        public PrefabInfo PrefabInfo { get; private set; }
        public Boolean IsLarge { get; private set; }

        public ResourceMonitor(PrefabInfo prefabInfo, bool isLarge = false)
        {
            this.PrefabInfo = prefabInfo;
            this.IsLarge = isLarge;
        }

        public RecipeData GetRecipe()
        {
            int numIngredients = 1;
            if (this.IsLarge)
                numIngredients = 2;
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Glass, numIngredients),
                    new Ingredient(TechType.ComputerChip, numIngredients),
                    new Ingredient(TechType.AdvancedWiringKit, numIngredients),
                },
            };
        }

        public GameObject CreateObject()
        {
            // Create the main game object for the Resource Monitor
            var screen = UnityEngine.Object.Instantiate(Plugin.RESOURCE_MONITOR_DISPLAY_MODEL);
            var screenModel = screen.transform.GetChild(0).gameObject;

            // Apply shader
            var shader = Shader.Find("MarmosetUBER");
            var renderers = screen.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.shader = shader;
            }

            var skyApplier = screen.AddComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            // Make it Buildable, using the separate construction model
            var constructable = screen.AddComponent<Constructable>();
            constructable.allowedOnWall = true;
            constructable.allowedOnCeiling = true;
            constructable.allowedInSub = true;
            constructable.allowedInBase = true;
            constructable.allowedOnGround = false;
            constructable.allowedOutside = false;
            constructable.allowedUnderwater = false;
            constructable.allowedOnConstructables = false;
            constructable.model = screenModel;
            constructable.techType = this.PrefabInfo.TechType;
            constructable.ExcludeFromSubParentRigidbody(); // LMAO im literally dying this fixed it.

            screen.AddComponent<TechTag>().type = this.PrefabInfo.TechType;
            screen.AddComponent<PrefabIdentifier>().ClassId = this.PrefabInfo.ClassID;
            screen.AddComponent<VFXSurface>();
            screen.AddComponent<Components.ResourceMonitorLogic>();

            // Modify scale of the model if needed
            if (this.IsLarge)
                screen.transform.localScale = LargeScale;

            // Return the main game object
            return screen;
        }
    }
}

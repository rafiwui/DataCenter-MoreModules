using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(DataCenter_MoreModules.MoreModules), "More Modules Mod", "1.0.0", "ASavageSwan")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace DataCenter_MoreModules;

public class MoreModules : MelonMod
{
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("===== More Modules Mod =====");
    }
    
    public static void UpgradeToBulkBox(SFPBox box, int newCapacity)
        {
            var oldPositions = box.sfpPositions;
            if (oldPositions == null || oldPositions.Length == 0) return;
            
            int oldCap = oldPositions.Length;
            if (oldCap >= newCapacity) return; 
            
            var newPositions = new Il2CppReferenceArray<Transform>(newCapacity);
            var newUsed = new Il2CppStructArray<int>(newCapacity);

            for (int i = 0; i < oldCap; i++)
            {
                newPositions[i] = oldPositions[i];
                newUsed[i] = box.usedPositions != null && i < box.usedPositions.Length ? box.usedPositions[i] : 0;
            }

            // New slots always start full: a purchased box is full, and a loaded box has its saved flags applied afterwards
            const int fullSlotValue = 1;
            
            for (int i = oldCap; i < newCapacity; i++)
            {
                // Find which of the original 5 slots this new one should sit directly on top of
                int baseColumnIndex = i % oldCap; // Loops 0, 1, 2, 3, 4
                int layerNumber = i / oldCap;     // Layer 1, 2, 3, 4

                Transform baseSlot = oldPositions[baseColumnIndex];

                // Clone the base slot
                GameObject newSlotObj = Object.Instantiate(baseSlot.gameObject, baseSlot.parent);
                newSlotObj.name = $"SFPPositionInBox_{i}";

                Vector3 baseLocalPos = baseSlot.localPosition;

                // Add a vertical offset based on what layer this is.
                // Since the original SFPs are spaced 0.03 apart horizontally, 
                // 0.035f is a great starting guess for their vertical thickness!
                float layerHeightOffset = layerNumber * 0.0f; 

                newSlotObj.transform.localPosition = new Vector3(
                    baseLocalPos.x, 
                    baseLocalPos.y + layerHeightOffset, 
                    baseLocalPos.z
                );

                // Add to our new arrays
                newPositions[i] = newSlotObj.transform;
                newUsed[i] = fullSlotValue; 
            }

            // Overwrite the box's internal memory
            box.sfpPositions = newPositions;
            box.usedPositions = newUsed;
            // The game serializes saveIntArray (an alias of usedPositions set in SFPBox.Awake), so it must follow the new array
            box.saveIntArray = newUsed;

            // Scale the cardboard box up visually so the new layers don't poke out the top!
            box.transform.localScale = new Vector3(
                box.transform.localScale.x, 
                box.transform.localScale.y ,
                box.transform.localScale.z
            );
        }
}
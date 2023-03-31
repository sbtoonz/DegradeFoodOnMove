using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace DegradeOnMove
{
    public class PatchClass
    {
        /*
         IL_0000: ldarg.0      // this
        IL_0001: ldarg.0      // this
        IL_0002: ldfld        float32 Player::m_foodUpdateTimer
        IL_0007: ldarg.1      // dt
        IL_0008: add
        IL_0009: stfld        float32 Player::m_foodUpdateTimer

         */

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
        public class FoodDegredationTranspiler
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(
                        useEnd: false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_foodUpdateTimer))),
                        new CodeMatch(OpCodes.Ldarg_1),
                        new CodeMatch(OpCodes.Add))
                    .Advance(offset: 3)
                    .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, float>>(CheckPlayerMovementSetFood))
                    .InstructionEnumeration(); 
            }
        }


        static float CheckPlayerMovementSetFood(float value)
        {
            if (!DegradeOnMoveMod.UseMod.Value) return value;
            if (Player.m_localPlayer.m_moveDir == Vector3.zero)
            {
                // <-- or whatever you need to check
                return 0f; // <-- adds 0 time 
            }

            return value;
        }
        
    }
}
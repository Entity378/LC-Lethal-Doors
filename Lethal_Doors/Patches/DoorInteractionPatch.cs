using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace Lethal_Doors.Patches
{
	[HarmonyPatch(typeof(HangarShipDoor))]
	internal class DoorInteractionPatch
	{
        private static readonly Vector3 doorPosition = new Vector3(-5.72f, 1.305f, -14.1f);

        private static float doorClosingTimer = -1f;

        private static readonly float doorClosingDuration = 0.3f;

        [HarmonyPostfix]
		[HarmonyPatch("Update")]
		private static void PostfixUpdate(HangarShipDoor __instance)
		{
            if (!StartOfRound.Instance.shipIsLeaving && StartOfRound.Instance.shipHasLanded)
            {
                if (IsDoorClosing(__instance))
                {
                    if (doorClosingTimer < 0f)
                    {
                        doorClosingTimer = 0f;
                    }
                    if (doorClosingTimer < doorClosingDuration && IsDoorClosed(__instance.shipDoorsAnimator))
                    {
                        CheckForPlayersAndApplyDamage();
                        CheckForEnemiesAndApplyDamage();
                    }
                    doorClosingTimer += Time.deltaTime;
                }
                else
                {
                    doorClosingTimer = -1f;
                }
            }
        }

		private static bool IsDoorClosing(HangarShipDoor door)
		{
			return door.doorPower < 1f;
		}

		private static void CheckForPlayersAndApplyDamage()
		{
			if (StartOfRound.Instance != null && StartOfRound.Instance.allPlayerScripts != null)
			{
				foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts)
				{
					if (playerControllerB != null && IsPlayerInDangerZone(playerControllerB))
					{
                        Debug.Log("[Lethal Doors Fixed] Player " + playerControllerB.playerUsername + " is in danger zone");
                        ApplyLethalDamageOrInjurePlayer(playerControllerB);
                        Debug.Log("[Lethal Doors Fixed] Added to affected list added");
                    }
				}
			}
		}

		private static bool IsPlayerInDangerZone(PlayerControllerB player)
		{
            float numX = 1f;
            float numY = 1.250f;
            float numZ = 1.250f;

            float playerPosX = player.transform.position.x;
            float playerPosY = player.transform.position.y;
            float playerPosZ = player.transform.position.z;

            Vector3 posX = new Vector3(playerPosX, 1.305f, -14.1f);
            Vector3 posY = new Vector3(-5.72f, playerPosY, -14.1f);
            Vector3 posZ = new Vector3(-5.72f, 1.305f, playerPosZ);

            float playerDistanceX = Vector3.Distance(posX, doorPosition);
            float playerDistanceY = Vector3.Distance(posY, doorPosition);
            float playerDistanceZ = Vector3.Distance(posZ, doorPosition);

            if (playerDistanceX < numX && playerDistanceY < numY && playerDistanceZ < numZ)
            {
                return true;
            }

            return false;
        }

		private static bool IsEnemyInDangerZone(EnemyAI enemy)
		{
            float numX = 3f;
            float numY = 2.250f;
            float numZ = 2.250f;

            float enemyPosX = enemy.transform.position.x;
			float enemyPosY = enemy.transform.position.y;
			float enemyPosZ = enemy.transform.position.z;

			Vector3 posX = new Vector3(enemyPosX, 1.305f, -14.1f);
			Vector3 posY = new Vector3(-5.72f, enemyPosY, -14.1f);
			Vector3 posZ = new Vector3(-5.72f, 1.305f, enemyPosZ);

			float enemyDistanceX = Vector3.Distance(posX, doorPosition);
			float enemyDistanceY = Vector3.Distance(posY, doorPosition);
			float enemyDistanceZ = Vector3.Distance(posZ, doorPosition);

            if (enemyDistanceX < numX && enemyDistanceY < numY && enemyDistanceZ < numZ) 
			{
                return true;
            }

			return false;
		}

		private static void ApplyLethalDamageOrInjurePlayer(PlayerControllerB player)
		{
			Debug.Log(string.Format("[Lethal Doors Fixed] {0} in danger zone, Position:{1} ", player.playerUsername, player.transform.position));
			Debug.Log("[Lethal Doors Fixed] Applying lethal damange to player " + player.playerUsername);
			if (player.criticallyInjured)
			{
				player.DamagePlayer(110, true, true, CauseOfDeath.Crushing, 0, false, default);
				Debug.Log(string.Format("[Lethal Doors Fixed] Heads: kill player ClientID:{0} ", (int)player.playerClientId));
			}
			else
			{
				Debug.Log(string.Format("[Lethal Doors Fixed] Tails: injure player ClientID:{0} ", (int)player.playerClientId));
				player.DamagePlayer(90, true, true, CauseOfDeath.Crushing, 0, false, default);
				player.AddBloodToBody();
				player.MakeCriticallyInjured(true);
			}
		}

		private static bool IsDoorClosed(Animator animator)
		{
			return animator.GetCurrentAnimatorStateInfo(0).IsName("ShipDoorClose");
		}

		private static void CheckForEnemiesAndApplyDamage()
		{
			List<EnemyAI> enemies = new List<EnemyAI>();
            Debug.Log("[Lethal Doors Fixed] Checking for enemies to apply damage");
            RoundManager roundManager = Object.FindObjectOfType<RoundManager>();
			if (roundManager != null && roundManager.SpawnedEnemies != null)
			{
                foreach (EnemyAI enemyAI in roundManager.SpawnedEnemies)
				{
                    if (enemyAI != null)
					{
						if(IsEnemyInDangerZone(enemyAI) && !enemyAI.isEnemyDead && !enemies.Contains(enemyAI))
						{
                            Debug.Log(string.Format("[Lethal Doors Fixed] {0} is in danger zone at position {1}", enemyAI.name, enemyAI.transform.position));
                            enemyAI.GetComponent<EnemyAI>().HitEnemyOnLocalClient(9999, new Vector3(0, 0, 0), null, false, -1);
							enemies.Add(enemyAI);
                            if (enemyAI.isEnemyDead)
                            {
                                Debug.Log("[Lethal Doors Fixed] " + enemyAI.name + " killed");
                            }
                        }
					}
				}
                enemies.Clear();
            }
		}
	}
}
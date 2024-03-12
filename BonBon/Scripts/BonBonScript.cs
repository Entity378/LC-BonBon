using BonBon.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BonBon.Scripts
{
    internal class BonBonScript : PhysicsProp
    {
        public List<AudioClip> UseVoicelinesSFX;
        public AudioClip errorSFX;
        public AudioClip useButtonSFX;
        public AudioClip dropSFX;
        public AudioClip grabSFX;

        private AudioSource audio;
        private float range = BonBonConfigs.BonBonRange;
        private int indexSFX = 0;

        public override void Start()
        {
            grabbable = true;
            grabbableToEnemies = false;
            insertedBattery = new Battery(false, 1);
            mainObjectRenderer = GetComponent<MeshRenderer>();
            useCooldown = BonBonConfigs.BonBonCooldown;
            audio = GetComponent<AudioSource>();
            base.Start();
        }

        public override void DiscardItem()
        {
            audio.Stop();
            audio.PlayOneShot(dropSFX);
            base.DiscardItem();
        }

        public override void GrabItem()
        {
            audio.Stop();
            audio.PlayOneShot(grabSFX);
            base.GrabItem();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            audio.Stop();
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null) 
            {
                if (insertedBattery.charge > 0)
                {
                    insertedBattery.charge = insertedBattery.charge - BonBonConfigs.BonBonBatteryUsage;

                    if (insertedBattery.charge < 0)
                    {
                        insertedBattery.charge = 0;
                    }

                    audio.PlayOneShot(useButtonSFX);
                    audio.PlayOneShot(UseVoicelinesSFX[indexSFX]);

                    if(indexSFX == 6)
                    {
                        indexSFX = 0;
                    }
                    else
                    {
                        indexSFX = indexSFX + 1;
                    }

                    var playerPos = transform.position;
                    float StunTime = BonBonConfigs.BonBonStunTime;

                    Collider[] hitColliders = Physics.OverlapSphere(playerPos, range);
                    int i = 0;
                    while (i < hitColliders.Length)
                    {
                        if (hitColliders[i].gameObject.CompareTag("Enemy"))
                        {
                            Debug.Log("BonBonLog: BonBon stunned enemy " + hitColliders[i].gameObject.GetComponentInParent<EnemyAI>());
                            EnemyAI Enemy = hitColliders[i].gameObject.GetComponentInParent<EnemyAI>();

                            StartCoroutine(StunEnemy(Enemy, StunTime));
                        }
                        i++;
                    }
                    Debug.Log("BonBonLog: BonBon used");
                }
                else 
                {
                    audio.PlayOneShot(errorSFX);
                    Debug.Log("BonBonLog: BonBon can't be used");
                }
            }
        }

        private IEnumerator StunEnemy(EnemyAI Enemy, float StunTime) 
        {
            Enemy.stunnedIndefinitely = 1;
            Enemy.SetEnemyStunned(true);
            Debug.Log("BonBonLog:" + Enemy.name + "Stunned");

            yield return new WaitForSeconds(StunTime);

            Enemy.stunnedIndefinitely = 0;
            Enemy.SetEnemyStunned(true, 0);
            Debug.Log("BonBonLog: " + Enemy.name + "NotStunned");
        }
    }
}
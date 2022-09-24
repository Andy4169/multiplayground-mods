using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;

namespace JoniMod
{
    public class JoniMod
    {
        public static void Main()
        {
            ModAPI.Register<CustomNpcDeathBehaviour>();

            //Stuff
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when it dies it gets sliced to pieces",
                    DescriptionOverride = "\"He just collapses into pieces to the ground\" -Scared Human",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    limb.Slice();

                                }
                            }
                        };
                    }
                }
             );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when it dies it dissapears",
                    DescriptionOverride = "\"No one will ever find out that hes dead\" -The Imposter",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    limb.Crush();

                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when it dies its bones break",
                    DescriptionOverride = "\"He slipped and broke his neck... No... i think he broke every single bone in his body.\" - Sad human",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    limb.BreakBone();
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when it dies its body parts freeze",
                    DescriptionOverride = "\"They just continued shooting beacuse he wasnt falling to the ground, and then they went closer and noticed that it was complelty frozen.\" - Criminal",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    limb.Frozen = true;
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when he dies he sparks",
                    DescriptionOverride = "\"He got stabbed and we saw a flash.\" - Witness",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    ModAPI.CreateParticleEffect("Spark", new Vector2(limb.transform.position.x, limb.transform.position.y));
                                    ModAPI.CreateParticleEffect("Flash", new Vector2(limb.transform.position.x, limb.transform.position.y));
                                    ModAPI.CreateParticleEffect("Vapor", new Vector2(limb.transform.position.x, limb.transform.position.y));
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when he dies he makes light",
                    DescriptionOverride = "\"He got shot... Saw a very bright red light... Couldnt see anything after that.\" - Blinded man",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    ModAPI.CreateLight(limb.transform, Color.red, 50f, 5f);
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when he dies he becomes a zombie",
                    DescriptionOverride = "\"He was running towards them, and got shot many times... but he still kept on running, and killed them...\" - Scared human",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    limb.IsZombie = true;
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when he dies he explodes",
                    DescriptionOverride = "\"3 kids were bullying a younger kid. Then they banged his head on the brick wall of the building and BOOM... All of them were dead.\" - Injured man",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                foreach (var limb in person.Limbs)
                                {
                                    ExplosionCreator.CreatePulseExplosion(limb.transform.position, 1, 10, true);
                                }
                            }
                        };
                    }
                }
            );
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Human"),
                    NameOverride = "Human but when he dies everyone else will die",
                    DescriptionOverride = "\"What happened? The last thing I saw was a gunshot and then what happened?\" - Ghost \"I have no idea. We just died.\" - Ghost 2",
                    CategoryOverride = ModAPI.FindCategory("Entities"),
                    ThumbnailOverride = ModAPI.LoadSprite("h1thumb.png"),
                    AfterSpawn = (Instance) =>
                    {
                        var skin = ModAPI.LoadTexture("skin.png");
                        var flesh = ModAPI.LoadTexture("flesh.png");
                        var bone = ModAPI.LoadTexture("bone.png");


                        var person = Instance.GetComponent<PersonBehaviour>();

                        person.SetBodyTextures(skin, flesh, bone, 1);


                        person.SetBruiseColor(86, 62, 130);
                        person.SetSecondBruiseColor(154, 0, 7);
                        person.SetThirdBruiseColor(207, 206, 120);
                        person.SetRottenColour(202, 199, 104);
                        person.SetBloodColour(108, 0, 4);

                        ModAPI.OnDeath += (sender, being) =>
                        {
                            if (being == person)
                            {
                                PersonBehaviour[] humans = UnityEngine.Object.FindObjectsOfType<PersonBehaviour>();
                                foreach (var human in humans)
                                {
                                    foreach (var limb in human.Limbs)
                                    {
                                        limb.Health = 0;
                                    }
                                }
                            }
                        };
                    }
                }
            );

            //Events

            ModAPI.OnDeath += delegate (object sender, PersonBehaviour npc)
            {
                if (CustomNpcDeathBehaviour.chdMode != 0)
                {
                    if (CustomNpcDeathBehaviour.chdMode == 1)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.Slice();
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 2)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.Crush();
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 3)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.BreakBone();
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 4)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.Frozen = true;
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 5)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            ModAPI.CreateParticleEffect("Spark", new Vector2(limb.transform.position.x, limb.transform.position.y));
                            ModAPI.CreateParticleEffect("Flash", new Vector2(limb.transform.position.x, limb.transform.position.y));
                            ModAPI.CreateParticleEffect("Vapor", new Vector2(limb.transform.position.x, limb.transform.position.y));
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 6)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            ModAPI.CreateLight(limb.transform, Color.red, 50f, 5f);
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 7)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.IsZombie = true;
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 8)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            ExplosionCreator.CreatePulseExplosion(limb.transform.position, 1, 10, true);
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 9)
                    {
                        PersonBehaviour[] humans = UnityEngine.Object.FindObjectsOfType<PersonBehaviour>();
                        foreach (var human in humans)
                        {
                            foreach (var limb in human.Limbs)
                            {
                                limb.Health = 0;
                            }
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 10)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.PhysicalBehaviour.MakeWeightless();
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 11)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.PhysicalBehaviour.Ignite(true);
                        }
                    }
                    if (CustomNpcDeathBehaviour.chdMode == 12)
                    {
                        foreach (var limb in npc.Limbs)
                        {
                            limb.PhysicalBehaviour.Selectable = false;
                        }
                    }
                }
            };
        }
    }
}
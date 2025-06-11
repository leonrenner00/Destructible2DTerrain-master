using UnityEngine;
                            
                            public class Projectile : MonoBehaviour
                            {
                                public int damage = 40;
                                [HideInInspector] public GameObject owner;
                            
                                public GameObject explosionPrefab;
                                public AudioClip[] explosionSounds; // Im Inspector füllen
                                public float explosionSoundPitch = 1.5f; // Im Inspector anpassen
                                public float explosionSoundVolume = 1.0f; // Lautstärke im Inspector anpassen (0.0 bis 1.0)
                            
                                private Collider2D ownerCollider;
                                private Collider2D thisCollider;
                                public float blastRadius = 5f;
                                private bool canDamageOwner = false;
                                private float ignoreOwnerTime = 0.5f;
                                private float timer = 0f;
                            
                                private void Start()
                                {
                                    if (owner != null)
                                    {
                                        ownerCollider = owner.GetComponent<Collider2D>();
                                        thisCollider = GetComponent<Collider2D>();
                            
                                        if (ownerCollider != null && thisCollider != null)
                                        {
                                            Physics2D.IgnoreCollision(thisCollider, ownerCollider, true);
                                        }
                                    }
                                    Destroy(gameObject, 15f);
                                }
                            
                                private void Update()
                                {
                                    timer += Time.deltaTime;
                            
                                    if (!canDamageOwner && timer > ignoreOwnerTime)
                                    {
                                        if (ownerCollider != null && thisCollider != null)
                                        {
                                            Physics2D.IgnoreCollision(thisCollider, ownerCollider, false);
                                            canDamageOwner = true;
                                        }
                                    }
                                }
                            
                                void OnCollisionEnter2D(Collision2D collision)
                                {
                                    if (collision.gameObject == owner && !canDamageOwner)
                                        return;
                            
                                    PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
                            
                                    if (player != null)
                                    {
                                        player.TakeDamage(damage);
                                    }
                                    var tm = FindObjectOfType<TileMapDestruction>();
                                    if (tm) tm.PunchHole(transform.position, blastRadius);
                                    // Explosion erzeugen
                                    if (explosionPrefab != null)
                                    {
                                        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                                        PlayExplosionSound(transform.position);
                                    }
                            
                                    Destroy(gameObject);
                                }
                            
                                void PlayExplosionSound(Vector3 position)
                                {
                                    if (explosionSounds != null && explosionSounds.Length > 0)
                                    {
                                        int idx = Random.Range(0, explosionSounds.Length);
                                        AudioSource source = new GameObject("TempAudio").AddComponent<AudioSource>();
                                        source.clip = explosionSounds[idx];
                                        source.pitch = explosionSoundPitch;
                                        source.volume = explosionSoundVolume; // Lautstärke setzen
                                        source.Play();
                                        source.transform.position = position;
                                        Destroy(source.gameObject, source.clip.length / explosionSoundPitch);
                                    }
                                }
                                
                                
                            }
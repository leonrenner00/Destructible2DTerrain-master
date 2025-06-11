// ChargedProjectileLauncher.cs
                
                using UnityEngine;
                using UnityEngine.UI;
                using System.Collections;
                
                [System.Serializable]
                public class ChargeEndSoundData
                {
                    public AudioClip[] sounds;
                    public float animationDuration = 0.7f;
                }
                
                public class ProjectileShooter : MonoBehaviour
                {
                    [Header("Projectile")]
                    public GameObject projectilePrefab;
                    public Transform shootPoint;
                    public float maxChargeTime = 2f;
                    public float maxShootForce = 20f;
                
                    [Header("UI")]
                    public Image chargeBarImage;
                
                    [Header("Sound")]
                    public ChargeEndSoundData chargeEndSoundData;
                    public float chargeEndVolume = 1.0f;
                    public AudioClip chargeLoopSound;
                    [Range(0f, 1f)]
                    public float chargeLoopVolume = 1.0f;
                    [Range(1f, 10f)]
                    public float chargeLoopMaxPitch = 4f;
                
                    private AudioSource audioSource;
                    private AudioSource chargeLoopSource;
                
                    private float chargeTime = 0f;
                    private float lastChargeTime = 0f;
                    private bool isCharging = false;
                
                    private Animator animator;
                
                    void Start()
                    {
                        animator = GetComponent<Animator>();
                        if (chargeBarImage != null)
                            chargeBarImage.enabled = false;
                
                        audioSource = gameObject.AddComponent<AudioSource>();
                        chargeLoopSource = gameObject.AddComponent<AudioSource>();
                        chargeLoopSource.loop = true;
                        chargeLoopSource.playOnAwake = false;
                        chargeLoopSource.volume = chargeLoopVolume;
                        chargeLoopSource.spatialBlend = 0f;
                    }
                
                    void Update()
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            BeginCharge();
                        }
                        if (Input.GetMouseButton(0))
                        {
                            ContinueCharge(Time.deltaTime);
                        }
                        if (Input.GetMouseButtonUp(0) && isCharging)
                        {
                            ReleaseAndShoot(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        }
                    }
                
                    public void BeginCharge()
                    {
                        isCharging = true;
                        chargeTime = 0f;
                
                        if (chargeBarImage != null)
                        {
                            chargeBarImage.enabled = true;
                            chargeBarImage.fillAmount = 0f;
                        }
                
                        // Ladesound starten
                        if (chargeLoopSound != null && !chargeLoopSource.isPlaying)
                        {
                            chargeLoopSource.clip = chargeLoopSound;
                            chargeLoopSource.Play();
                        }
                    }
                
                    public void ContinueCharge(float deltaTime)
                    {
                        if (!isCharging) return;
                
                        chargeTime += deltaTime;
                        chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
                
                        // Ladesound anpassen
                        if (chargeLoopSource.isPlaying)
                        {
                            float t = chargeTime / maxChargeTime;
                            chargeLoopSource.pitch = Mathf.Lerp(1f, chargeLoopMaxPitch, t);
                            chargeLoopSource.volume = chargeLoopVolume;
                        }
                
                        if (animator != null)
                            animator.SetFloat("chargeBlend", chargeTime / maxChargeTime);
                
                        if (chargeBarImage != null)
                        {
                            float t = chargeTime / maxChargeTime;
                            chargeBarImage.fillAmount = t;
                            chargeBarImage.color = t < 0.5f
                                ? Color.Lerp(Color.green, Color.red, t * 2)
                                : Color.Lerp(Color.red, Color.blue, (t - 0.5f) * 2);
                        }
                    }
                
                    public bool ReleaseAndShoot(Vector2 targetWorldPos)
                    {
                        if (!isCharging) return false;
                
                        // Ladesound stoppen
                        if (chargeLoopSource.isPlaying)
                            chargeLoopSource.Stop();
                
                        if (animator != null)
                        {
                            animator.SetTrigger("throw");
                            StartCoroutine(ResetChargeBlend());
                        }
                
                        lastChargeTime = chargeTime;
                        chargeTime = 0f;
                        isCharging = false;
                
                        if (chargeBarImage != null)
                        {
                            chargeBarImage.fillAmount = 0f;
                            chargeBarImage.enabled = false;
                        }
                
                        ShootProjectile(targetWorldPos);
                        return true;
                    }
                
                    private void ShootProjectile(Vector2 targetWorldPos)
                    {
                        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
                
                        Projectile projScript = projectile.GetComponent<Projectile>();
                        if (projScript != null)
                        {
                            projScript.owner = this.gameObject;
                        }
                
                        Vector2 shootDirection = ((Vector2)targetWorldPos - (Vector2)shootPoint.position).normalized;
                        float force = (lastChargeTime / maxChargeTime) * maxShootForce;
                
                        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.velocity = Vector2.zero;
                            rb.AddForce(shootDirection * force, ForceMode2D.Impulse);
                        }
                    }
                
                    IEnumerator ResetChargeBlend()
                    {
                        yield return new WaitForSeconds(0.3f);
                        if (animator != null)
                            animator.SetFloat("chargeBlend", 0f);
                    }
                
                    // Diese Methode im Animation Event aufrufen
                    public void PlayChargeEndSound()
                    {
                        if (chargeEndSoundData != null && chargeEndSoundData.sounds != null && chargeEndSoundData.sounds.Length > 0)
                        {
                            int idx = Random.Range(0, chargeEndSoundData.sounds.Length);
                            AudioClip clip = chargeEndSoundData.sounds[idx];
                            float pitch = clip.length > 0f ? clip.length / chargeEndSoundData.animationDuration : 1f;
                            audioSource.pitch = pitch;
                            audioSource.PlayOneShot(clip, chargeEndVolume);
                            StartCoroutine(ResetPitch());
                        }
                    }
                
                    private IEnumerator ResetPitch()
                    {
                        yield return null;
                        audioSource.pitch = 1f;
                    }
                }
using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : SingletonMonoBehaviour<FirstPersonController>
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
    [Space]
    public float smallStepTime = 0.1f;
    public float bigStepTime = 0.5f;
    public bool waitsOnStart = true;

    private WeaponHolder _weaponHolder;
    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;

    private bool _isWaiting = false;
    private bool _isScrollKeyDown;
    private float _lastStep;
    private float _stepTime;
    private Camera _mainCamera;
    private int _previousWordIndex;
    private int _wordIndex;
    private int _poemIndex;
    public Poem ActivePoem
    {
        get
        {
            return GameManager.Instance.wordCollection.poems[_poemIndex];
        }
    }

    public Gun Gun { get; set; }

    public int WordIndex
    {
        get
        {
            return _wordIndex;
        }
        set
        {
            if (_wordIndex != value)
            {
                _wordIndex = value;

                if (_wordIndex < 0)
                    _wordIndex = ActivePoem.words.Count - 1;
                else if (_wordIndex > ActivePoem.words.Count - 1)
                    _wordIndex = 0;

                RefreshSelection();
            }
        }
    }
    private float _selectionChangeThreshold = 0f;
    private CameraShake _cameraShake;
    private float _selectionTimer;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);

        _mainCamera = Camera.main;
        _cameraShake = _mainCamera.GetComponent<CameraShake>();
        _weaponHolder = GetComponentInChildren<WeaponHolder>();

        if (waitsOnStart)
            StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        _isWaiting = true;

        yield return new WaitForSeconds(1f);

        _isWaiting = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isWaiting)
            return;

        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        UpdateClick();

        UpdateScroll();
    }

    private void UpdateScroll()
    {
        if (!Gun)
            return;

        var scrollValue = Input.GetAxis("Mouse ScrollWheel");

        var indexDelta = 0;

        if (Input.GetKey(KeyCode.E))
            indexDelta = 1;
        else if (Input.GetKey(KeyCode.Q))
            indexDelta = -1;
        else if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q))
            _stepTime = 0f;

        if (scrollValue > 0f)
        {
            indexDelta = 1;
            _stepTime = smallStepTime;
        }
        else if (scrollValue < 0f)
        {
            indexDelta = -1;
            _stepTime = smallStepTime;
        }

        if (indexDelta != 0)
        {
            if (Time.time - _lastStep > _stepTime)
            {
                _lastStep = Time.time;
                _stepTime = _isScrollKeyDown ? smallStepTime : bigStepTime;
                _isScrollKeyDown = true;
                WordIndex += indexDelta;
            }
        }
        else
        {
            _isScrollKeyDown = false;
        }
    }

    private void RefreshSelection()
    {
        if (_previousWordIndex != WordIndex)
            PoemHUD.Instance.Deselect(_previousWordIndex);

        PoemHUD.Instance.Select(WordIndex);

        _previousWordIndex = WordIndex;
    }

    public void CyclePoem()
    {
        _poemIndex++;
        if (_poemIndex > GameManager.Instance.wordCollection.poems.Count - 1)
            _poemIndex = 0;

        PoemHUD.Instance.Poem = ActivePoem;

        StartCoroutine(LateSelect());
    }

    private IEnumerator LateSelect()
    {
        yield return null;

        WordIndex = 0;
        PoemHUD.Instance.Select(WordIndex);
    }

    private void UpdateClick()
    {
        if (Input.GetMouseButton(0))
        {
            if (Gun && Gun.IsROFReady)
            {
                Shoot();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Gun)
            {
                Gun.HasReleasedTrigger = true;
            }
        }
    }

    private void Shoot()
    {
        Gun.Shoot(_wordIndex);

        _cameraShake.shakeDuration = .05f;
    }

    public Gun Equip(GunPickup gunPickup)
    {
        var newGun = gunPickup.Gun;
        gunPickup.Gun = null;

        var previousGun = Gun;
        if (Gun != null)
        {
            Gun.Unequip(gunPickup);
        }

        newGun.transform.parent = _weaponHolder.transform;
        newGun.transform.localPosition = Vector3.zero;
        newGun.transform.parent = _weaponHolder.weaponPivot;
        newGun.transform.localRotation = Quaternion.identity;
        newGun.Equip();

        PoemHUD.Instance.Poem = ActivePoem;

        Gun = newGun;

        return previousGun;
    }

    public IEnumerator RefreshAtEndOfFrame()
    {
        yield return null;

        PoemHUD.Instance.Select(_wordIndex);
    }

    public void StepGun()
    {
        if (!Gun)
            return;

        _weaponHolder.Step();
    }


    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }


    private void FixedUpdate()
    {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();
    }


    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }

        _weaponHolder.Step();

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        //_weaponHolder.LeadTurn(m_Input);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }


    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
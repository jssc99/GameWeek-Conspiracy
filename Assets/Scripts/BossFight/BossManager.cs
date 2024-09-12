using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    private BossDash m_DashScrpt = null;

    private BossProjectile m_ProjScrpt = null;

    private AOESpawnManager m_AOESpawnManager = null;

    // turn to true when starting an attack, gets turned to false by the other scripts
    [HideInInspector] public bool IsBossBussy = false;

    [HideInInspector] public bool IsBossVulnerable = false;

    public int Health = 10;

    [Header("Boss Phases")]
    [Range(1, 3)] public int CurrentBossPhase = 1;

    [SerializeField] private int m_HealthEndFirstPhase = 8;
    [SerializeField] private int m_HealthEndSecondPhase = 5;

    private enum ACTIONS
    {
        AOE_Random,
        AOE_Wave,
        AOE_Follow,
        Shoot_Player,
        Shoot_Wave,
        Dash_Random,
        Dash_Player,
        First_Movement,
    }

    private List<ACTIONS> m_Previous; // latest actions get added to 0

    private List<ACTIONS> m_SecondWavePool;
    private List<ACTIONS> m_ThirdWavePool;

    [SerializeField] private int m_NbBulletsAgainstPlayer = 10;

    private void Start()
    {
        if (!TryGetComponent(out m_DashScrpt)) Debug.LogError("BossDash script not found in BossManager");
        if (!TryGetComponent(out m_ProjScrpt)) Debug.LogError("BossProjectile script not found in BossManager");
        if (!TryGetComponent(out m_AOESpawnManager)) Debug.LogError("AOESpawnManager script not found in BossManager");

        m_Previous = new()
        {
            ACTIONS.First_Movement
        };
    }

    private void Update()
    {
        if (IsBossBussy || IsBossVulnerable) return;

        switch (CurrentBossPhase)
        {
            case 1:
                FirstPhaseAction(); break;
            case 2:
                SecondPhaseAction(); break;
            case 3:
                ThirdPhaseAction(); break;
            default:
                Debug.Log("Boss phase not handled in BossManager::Update, Nb:" + CurrentBossPhase);
                break;
        }
    }

    private bool IsPreviousActionAOE(int _ID = 0)
    {
        if (_ID > m_Previous.Count) return false;
        return m_Previous[_ID] == ACTIONS.AOE_Random || m_Previous[_ID] == ACTIONS.AOE_Wave || m_Previous[_ID] == ACTIONS.AOE_Follow;
    }

    private bool IsPreviousActionShoot(int _ID = 0)
    {
        if (_ID > m_Previous.Count) return false;
        return m_Previous[_ID] == ACTIONS.Shoot_Player || m_Previous[_ID] == ACTIONS.Shoot_Wave;
    }

    private bool IsPreviousActionDash(int _ID = 0)
    {
        if (_ID > m_Previous.Count) return false;
        return m_Previous[_ID] == ACTIONS.Dash_Player || m_Previous[_ID] == ACTIONS.Dash_Random;
    }

    private void FirstPhaseAction()
    {
        if (m_Previous[0] == ACTIONS.First_Movement || IsPreviousActionDash())
            MakeAction(ACTIONS.AOE_Random);
        else if (IsPreviousActionAOE())
            MakeAction(ACTIONS.Shoot_Player);
        else if (IsPreviousActionShoot())
            MakeAction(ACTIONS.Dash_Random);
        else
            Debug.LogError("FIRST WAVE: Previous ACTION not handled");
    }

    private ACTIONS GetSecondPhaseAction()
    {
        if (m_SecondWavePool == null || m_SecondWavePool.Count == 0)
        {
            m_SecondWavePool = new()
            {
                ACTIONS.AOE_Random,
                ACTIONS.AOE_Wave,
                ACTIONS.Shoot_Player,
                ACTIONS.Shoot_Wave
            };
        }

        int IDAction = Random.Range(0, m_SecondWavePool.Count);
        ACTIONS returnAction = m_SecondWavePool[IDAction];
        m_SecondWavePool.RemoveAt(IDAction);

        return returnAction;
    }

    private void SecondPhaseAction()
    {
        if (m_Previous[0] == ACTIONS.First_Movement)
            MakeAction(GetSecondPhaseAction());
        else if (!IsPreviousActionDash() && !IsPreviousActionDash(1))
            // if last two actions are ATTACKS, DASH is unlocked
            MakeAction(ACTIONS.Dash_Player);
        else
            MakeAction(GetSecondPhaseAction());
    }

    private ACTIONS GetThirdPhaseAction()
    {
        if (m_ThirdWavePool == null || m_ThirdWavePool.Count == 0)
        {
            m_ThirdWavePool = new()
            {
                ACTIONS.AOE_Wave,
                ACTIONS.AOE_Follow,
                ACTIONS.AOE_Random,
                ACTIONS.Shoot_Player,
                ACTIONS.Shoot_Wave
            };
        }

        int IDAction = Random.Range(0, m_ThirdWavePool.Count);
        ACTIONS returnAction = m_ThirdWavePool[IDAction];
        m_ThirdWavePool.RemoveAt(IDAction);

        return returnAction;
    }

    private void ThirdPhaseAction()
    {
        if (m_Previous[0] == ACTIONS.First_Movement)
            MakeAction(GetThirdPhaseAction());
        else if (!IsPreviousActionDash() && !IsPreviousActionDash(1))
            // if last two actions are ATTACKS, DASH is unlocked
            if (Random.Range(0, 2) == 0)
                MakeAction(ACTIONS.Dash_Random);
            else
                MakeAction(ACTIONS.Dash_Player);
        else
            MakeAction(GetThirdPhaseAction());
    }

    private void MakeAction(ACTIONS _theAction)
    {
        switch (_theAction)
        {
            case ACTIONS.AOE_Random:
                Debug.Log("AOE_Random launched");
                m_Previous.Insert(0, ACTIONS.AOE_Random);
                return; // TODELETE
                break;

            case ACTIONS.AOE_Wave:
                Debug.Log("AOE_Wave launched");
                m_Previous.Insert(0, ACTIONS.AOE_Wave);
                return; // TODELETE
                break;

            case ACTIONS.AOE_Follow:
                Debug.Log("AOE_Follow launched");
                m_Previous.Insert(0, ACTIONS.AOE_Follow);
                return; // TODELETE
                break;

            case ACTIONS.Shoot_Player:
                m_ProjScrpt.ShootPlayer(m_NbBulletsAgainstPlayer);
                m_Previous.Insert(0, ACTIONS.Shoot_Player);
                break;

            case ACTIONS.Shoot_Wave:
                m_ProjScrpt.ShootWave();
                m_Previous.Insert(0, ACTIONS.Shoot_Wave);
                break;

            case ACTIONS.Dash_Random:
                m_DashScrpt.DashToWaypoint();
                m_Previous.Insert(0, ACTIONS.Dash_Random);
                break;

            case ACTIONS.Dash_Player:
                m_DashScrpt.DashToPlayer();
                m_Previous.Insert(0, ACTIONS.Dash_Player);
                break;

            case ACTIONS.First_Movement:
            default:
                Debug.Log("MakeAction ACTION not handled");
                return;
        }
        IsBossBussy = true;
    }

    private void NextPhase()
    {
        m_Previous = new()
        {
            ACTIONS.First_Movement
        };

        CurrentBossPhase++;
        Debug.Log("NEW BOSS PHASE");
    }

    public void TakeDamage(int _dmg)
    {
        Health -= _dmg;
        if (Health <= 0)
            Destroy(gameObject);
        else if ((Health <= m_HealthEndSecondPhase && CurrentBossPhase == 2)
            || (Health <= m_HealthEndFirstPhase && CurrentBossPhase == 1))
            NextPhase();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float m_Speed;

    private Vector3 m_Direction;

    private float m_HorizontalInput;
    private float m_VerticalInput;

    private string m_HorizontalAxis = "Horizontal";
    private string m_VerticalAxis = "Vertical";
    private string m_Dash = "Dash";

    private bool m_IsDashing;
    [SerializeField, Range(1f, 10f)] private float m_DashPower;
    [SerializeField] private float m_TimeBtwDashes = 1f;
    [SerializeField] private float m_DashDuration = 0.5f;
    private float m_IsDashingTimer;
    private float m_DashTimer;
    private float m_DashMultiplicator = 1f;
    private float m_DashMultiplicatorBaseValue = 1f;

    public bool m_IsPlayerVulnerable = true;

    public bool IsPlayerVulnerable { get { return m_IsPlayerVulnerable; } set { m_IsPlayerVulnerable = value; } }

    private Vector3 m_PreviousPos;
    private float m_DistanceFromCenter;
    private float m_MaxDistanceFromCenter;

    [SerializeField] private Transform m_ArenaCenterPos;

    [SerializeField] private Animator m_Animator;

    private void Start()
    {
        m_DashTimer = m_TimeBtwDashes;
        m_PreviousPos = transform.position;
        m_MaxDistanceFromCenter = m_ArenaCenterPos.localScale.x/2;
    }

    // Update is called once per frame
    void Update()
    {
        m_HorizontalInput = Input.GetAxis(m_HorizontalAxis);
        m_VerticalInput = Input.GetAxis(m_VerticalAxis);

        m_Direction = new Vector3(m_HorizontalInput, 0, m_VerticalInput);
        m_Direction = m_Direction.normalized;

        Dash();

        transform.position += m_Direction * m_Speed * m_DashMultiplicator * Time.deltaTime;

        m_DistanceFromCenter = Vector3.Distance(m_ArenaCenterPos.position, transform.position);

        if (m_DistanceFromCenter > m_MaxDistanceFromCenter)
        {
            transform.position = m_PreviousPos;
        }

        m_PreviousPos = transform.position;

        m_DashTimer += Time.deltaTime;

        m_Animator.SetFloat("Speed", Mathf.Abs(m_Direction.x));
    }

    private void Dash()
    {
        if (Input.GetButtonDown(m_Dash) && m_DashTimer >= m_TimeBtwDashes && !m_IsDashing)
        {
            m_DashMultiplicator = m_DashPower;
            m_IsDashing = true;
            m_IsPlayerVulnerable = false;
        }

        if (m_IsDashing)
        {
            m_IsDashingTimer += Time.deltaTime;

            if (m_IsDashingTimer >= m_DashDuration)
            {
                m_IsDashing = false;
                m_IsPlayerVulnerable = true;
                m_IsDashingTimer = 0f;
                m_DashTimer = 0f;
                m_DashMultiplicator = m_DashMultiplicatorBaseValue;
            }
        }
    }
}
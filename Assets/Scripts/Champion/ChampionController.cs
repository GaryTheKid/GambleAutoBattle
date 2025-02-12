using UnityEngine;
using Unity.Netcode;

public class ChampionController : NetworkBehaviour
{
    public bool teamId;
    public float speed = 5f;
    public short hp = 500;

    private CharacterController characterController;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        BattleSimulator_Server.Instance.SetChampion(this, OwnerClientId);
        UpdateTeam((byte)OwnerClientId);
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveY);
        characterController.Move(movement * speed);
    }

    public void UpdateTeam(byte teamId)
    {
        if (teamId != 1)
        {
            this.teamId = true;
            meshRenderer.material.color = Color.red;
        }
        else
        {
            this.teamId = false;
            meshRenderer.material.color = Color.blue;
        }
    }

    public Vector2 GetPositionXZ()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public UnitState ToUnitState()
    {
        return new UnitState(
            (ushort)OwnerClientId,  // Use OwnerClientId as the unique ID
            GetPositionXZ(),        // Convert world position (XZ) to Vector2
            (ushort)hp,             // Convert HP to ushort
            teamId                  // Pass the team ID
        );
    }
}

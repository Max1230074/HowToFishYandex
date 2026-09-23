using UnityEngine;

public class Shopkeeper : MonoBehaviour
{
    private PlayerController player;
    private float hintTimer;

    private void Start()
    {
        player = PlayerController.Instance;
    }

    private void Update()
    {
        if (player == null && PlayerController.Instance != null) player = PlayerController.Instance;
        if (player == null) return;
        bool near = Vector2.Distance(transform.position, player.transform.position) <= 2.6f;
        player.InShopZone = near;

        hintTimer -= Time.deltaTime;
        if (near && hintTimer <= 0f)
        {
            hintTimer = 4f;
            if (HUD.Instance != null)
            {
                if (player.carriedFish == null) HUD.Instance.Hint("Рядом лавка. E — открыть");
                else HUD.Instance.Hint("E — продать рыбу (стоит ~$" + player.carriedFish.computeValue + ")");
            }
        }
    }
}
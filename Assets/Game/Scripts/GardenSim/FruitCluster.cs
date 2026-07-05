using UnityEngine;

/// <summary>
/// Sits on the root of an immovable multi-fruit growth stage (e.g. a tomato branch or an apple tree)
/// that holds several <see cref="FruitGrab"/> children. Once every fruit has been picked (each one
/// unparents itself on grab), destroys the whole <see cref="PlantGrowth"/> root, clearing the leftover
/// branch/trunk out of the GrowBox and freeing it for replanting.
/// </summary>
public class FruitCluster : MonoBehaviour
{
    private int _remainingFruit;

    private void Awake()
    {
        _remainingFruit = GetComponentsInChildren<FruitGrab>(true).Length;
    }

    /// <summary>Called by a <see cref="FruitGrab"/> child when it's grabbed and detaches.</summary>
    public void NotifyFruitPicked()
    {
        _remainingFruit--;
        if (_remainingFruit > 0) return;

        var plantGrowth = GetComponentInParent<PlantGrowth>();
        Destroy(plantGrowth != null ? plantGrowth.gameObject : gameObject);
    }
}

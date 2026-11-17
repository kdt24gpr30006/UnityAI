using UnityEngine;
using UnityEngine.InputSystem;

public class GameLogic : MonoBehaviour
{
    public int count = 0;
    private bool clearShown = false;
    private System.Collections.Generic.HashSet<string> collected = new System.Collections.Generic.HashSet<string>();
    private System.Collections.Generic.Dictionary<string, Vector3> initialPositions = new System.Collections.Generic.Dictionary<string, Vector3>();
    private System.Collections.Generic.Dictionary<string, GameObject> itemRefs = new System.Collections.Generic.Dictionary<string, GameObject>();

    void Awake()
    {
        initialPositions["Player"] = transform.position;
        for (int i = 1; i <= 5; i++)
        {
            var item = GameObject.Find("Item_" + i);
            if (item != null)
            {
                itemRefs[item.name] = item; // keep references: Find() cannot see inactive items
                initialPositions[item.name] = item.transform.position;
            }
        }
    }

    private bool rWasDown = false;

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        bool down = kb.rKey.isPressed; // edge-detect: works with state-write injection too
        if (down && !rWasDown) ResetAll(); // B5: R reset
        rWasDown = down;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.name.StartsWith("Item_")) return;
        if (collected.Contains(other.name)) return; // B3: no double counting
        collected.Add(other.name);
        count++;
        other.gameObject.SetActive(false); // item disappears
        if (count >= 5) clearShown = true; // B4: CLEAR only when all 5
    }

    void ResetAll()
    {
        transform.position = initialPositions["Player"];
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero; // B5: velocity too
        count = 0;
        clearShown = false;
        collected.Clear();
        foreach (var kv in initialPositions)
        {
            if (kv.Key == "Player") continue;
            var item = itemRefs[kv.Key]; // stored references, not Find()
            if (item != null)
            {
                item.SetActive(true);
                item.transform.position = kv.Value;
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 400, 30), "Collect: " + count + " / 5"); // B1: shows 0 at start
        if (clearShown) GUI.Label(new Rect(20, 60, 400, 40), "CLEAR");       // hidden mid-collection
    }
}

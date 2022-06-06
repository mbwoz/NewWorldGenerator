using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

    CollectablesManager collectablesManager;
    PlayerMovement playerMovement;
    PopUpOverlay popUpOverlay;

    void Start() {
        collectablesManager = (CollectablesManager) FindObjectOfType(typeof(CollectablesManager));
        playerMovement = (PlayerMovement) FindObjectOfType(typeof(PlayerMovement));
        popUpOverlay = (PopUpOverlay) FindObjectOfType(typeof(PopUpOverlay));
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            save();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            load();
        }
    }

    private void save() {
        GameData data = new GameData();

        data.SetScore(collectablesManager.GetScore());
        data.SetCollectables(collectablesManager.GetCollectables());
        data.SetPlayer(playerMovement.transform.position);

        SaveSystem.SaveGame(data);
        popUpOverlay.SetPopUp("SAVED");
    }

    private void load() {
        GameData data = SaveSystem.LoadGame();
        if (data == null) return;

        collectablesManager.SetScore(data.GetScore());
        collectablesManager.SetCollectables(data.GetCollectables());
        playerMovement.transform.position = data.GetPlayer();

        popUpOverlay.SetPopUp("LOADED");
    }
}

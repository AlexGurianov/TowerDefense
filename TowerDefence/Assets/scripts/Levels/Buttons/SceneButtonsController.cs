﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneButtonsController : MonoBehaviour {

    public Camera cam;

    public PauseMenuController pauseMenuController;

    public GameObject deleteTowerButton;
    public GameObject upgradeTowerButton;
    public GameObject upgradeInfoPanel;

    public GameObject wallDirectionPanel;

    public GameObject tower1;
    public GameObject tower2;
    public GameObject tower3;
    public GameObject tree;
    public GameObject tomb;
    public GameObject wall;

    bool towerSelectionStays = false;

    public int selectedTowerID = -1;

    // Use this for initialization
    void Start () {
        deleteTowerButton.SetActive(false);
        upgradeTowerButton.SetActive(false);
        wallDirectionPanel.SetActive(false);
        upgradeInfoPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (wallDirectionPanel.activeSelf && Input.GetMouseButton(0) && !RectTransformUtility.RectangleContainsScreenPoint(
                 wallDirectionPanel.GetComponent<RectTransform>(), Input.mousePosition))
            wallDirectionPanel.SetActive(false);
        if (selectedTowerID != -1 && Input.GetMouseButton(0) && !towerSelectionStays && !RectTransformUtility.RectangleContainsScreenPoint(
                 deleteTowerButton.GetComponent<RectTransform>(), Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(
                 upgradeTowerButton.GetComponent<RectTransform>(), Input.mousePosition))
        {
            DeselectTower();
        }
    }

    public void PauseButtonClicked()
    {
        pauseMenuController.CallPauseMenu();
    }

    public void AddWallButtonClicked()
    {
        if (!DataStorage.dataStorage.isPlacingTower)
            wallDirectionPanel.SetActive(true);
    }

    public void AddTowerButtonClicked()
    {
        if (!DataStorage.dataStorage.isPlacingTower)
        {
            GameObject prefab;
            TowerType type;
            Quaternion rotation;
            switch (EventSystem.current.currentSelectedGameObject.name)
            {
                case "New Tower1 Button":
                default:
                    prefab = tower1;
                    type = TowerType.tower1;
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "New Tower2 Button":
                    prefab = tower2;
                    type = TowerType.tower2;
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "New Tower3 Button":
                    prefab = tower3;
                    type = TowerType.tower3;
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "New Tree Button":
                    prefab = tree;
                    type = TowerType.tree;
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "New Tomb Button":
                    prefab = tomb;
                    type = TowerType.tomb;
                    rotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case "Wall Up Button":
                    prefab = wall;
                    type = TowerType.wall_up;
                    rotation = Quaternion.Euler(0, 90, 0);
                    wallDirectionPanel.SetActive(false);
                    break;
                case "Wall Right Button":
                    prefab = wall;
                    type = TowerType.wall_right;
                    rotation = Quaternion.Euler(0, 180, 0);
                    wallDirectionPanel.SetActive(false);
                    break;
                case "Wall Down Button":
                    prefab = wall;
                    type = TowerType.wall_down;
                    rotation = Quaternion.Euler(0, 270, 0);
                    wallDirectionPanel.SetActive(false);
                    break;
                case "Wall Left Button":
                    prefab = wall;
                    type = TowerType.wall_left;
                    rotation = Quaternion.Euler(0, 0, 0);
                    wallDirectionPanel.SetActive(false);
                    break;
            }
            GameObject tower = (GameObject)Instantiate(prefab, new Vector3(0f, 0f, 0f), rotation);
            tower.GetComponent<TowerController>().type = type;
            tower.GetComponent<TowerController>().preset = false;
            switch (type)
            {
                case TowerType.tower1:
                case TowerType.tower2:
                case TowerType.tower3:
                    tower.GetComponent<TowerController>().level = 1;
                    tower.GetComponent<ShootingTowerController>().towerAtLevel =
                        GameObject.Find("UpgradeController").GetComponent<UpgradeController>().towersLevelInfo[type][0];
                    break;
            }
            DataStorage.dataStorage.isPlacingTower = true;
        }
    }

    public void DeleteTowerButtonClicked()
    {
        if (selectedTowerID != -1)
        {
            DataStorage.dataStorage.towersDictionary[selectedTowerID].DeleteTower();
            selectedTowerID = -1;
            deleteTowerButton.SetActive(false);
            upgradeTowerButton.SetActive(false);
            upgradeInfoPanel.SetActive(false);
            GameObject gameController = GameObject.Find("GameController");
            if (gameController != null)
                StartCoroutine(gameController.GetComponent<GameController>().UpdateMonstersPaths());
        }
    }

    public void UpgradeTowerButtonClicked()
    {
        if (selectedTowerID != -1)
        {
            DataStorage.dataStorage.towersDictionary[selectedTowerID].GetComponent<ShootingTowerController>().Upgrade();
            DeselectTower();
        }
    }

    public void DeselectTower()
    {
        selectedTowerID = -1;
        deleteTowerButton.SetActive(false);
        upgradeTowerButton.SetActive(false);
        upgradeInfoPanel.SetActive(false);
    } 

    public IEnumerator SelectTower(int TowerID)
    {
        if (TowerID != selectedTowerID)
        {
            towerSelectionStays = false;
            DeselectTower();
            if (!DataStorage.dataStorage.towersDictionary[TowerID].preset)
            {
                deleteTowerButton.SetActive(true);
                Vector3 towerPos = DataStorage.dataStorage.towersDictionary[TowerID].transform.position;
                towerPos.x -= DataStorage.dataStorage.towersDictionary[TowerID].GetComponent<Collider>().bounds.size.x * 0.2f;
                towerPos.y += DataStorage.dataStorage.towersDictionary[TowerID].GetComponent<Collider>().bounds.size.y;
                Vector3 crossPos = cam.WorldToScreenPoint(towerPos);
                deleteTowerButton.transform.position = crossPos;
            }
            if ((DataStorage.dataStorage.towersDictionary[TowerID].type == TowerType.tower1 ||
                DataStorage.dataStorage.towersDictionary[TowerID].type == TowerType.tower2 ||
                DataStorage.dataStorage.towersDictionary[TowerID].type == TowerType.tower3)
                && DataStorage.dataStorage.towersDictionary[TowerID].GetComponent<ShootingTowerController>().CanUpgrade() && !SceneInfoCarrier.sceneInfoCarrier.OpenNewMap)
            {
                MakeUpgradeInfoPanel(TowerID);
                upgradeTowerButton.SetActive(true);
                Vector3 towerPos = DataStorage.dataStorage.towersDictionary[TowerID].transform.position;
                towerPos.x += DataStorage.dataStorage.towersDictionary[TowerID].GetComponent<Collider>().bounds.size.x*0.8f;
                towerPos.y += DataStorage.dataStorage.towersDictionary[TowerID].GetComponent<Collider>().bounds.size.y;
                Vector3 updatePos = cam.WorldToScreenPoint(towerPos);
                upgradeTowerButton.transform.position = updatePos;
            }
            while (Input.GetMouseButton(0))
                yield return null;
            selectedTowerID = TowerID;
        }
        else
            towerSelectionStays = true;
    }

    void MakeUpgradeInfoPanel(int ID)
    {
        GameObject.Find("UpgradeController").GetComponent<UpgradeController>().UpdateUpgradeInfo(DataStorage.dataStorage.towersDictionary[ID].type, DataStorage.dataStorage.towersDictionary[ID].level);
        upgradeInfoPanel.SetActive(true);
    }
}

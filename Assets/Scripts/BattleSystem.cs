﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST };

public class BattleSystem : MonoBehaviour {

    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    public Unit playerUnit;
    public Unit enemyUnit;

    public Text dialogueText;
    public Text turnText;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;


    public Button option1;
    public Button option2;
    public Button option3;
    public Button option4;

    public BattleState state;

    private int correctAnswer;
    private int last_pos;

    void Start() {
        state = BattleState.START;
        last_pos = -1;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle() {
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGO.GetComponent<Unit>();

        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "A wild " + enemyUnit.unitName+" attacks!";

        playerUnit.SetInfo();
        enemyUnit.SetInfo();
        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        StartCoroutine(PlayerTurn());
    }

    //
    //
    //TRANSFORMAR O TURNO DO PLAYER E O DO INIMIGO NUM MÉTODO SÓ E USAR CONDICIONAL PRA MOSTRAR O TEXTO CORRETO/CHAMAR A FUNÇÃO CORRETA
    //
    //
    IEnumerator EnemyTurn() {
        dialogueText.text = "DEFENSE TURN";
        turnText.text = "DEFENSE TURN";
        yield return new WaitForSeconds(1f);
        PrepareAnswerPhase();
        changeActionButtonState(true);
    }

    IEnumerator EnemyAttack(int option) {
        dialogueText.text = enemyUnit.unitName + " attacks!";
        yield return new WaitForSeconds(1f);
        bool isDead = false;
        if(option != correctAnswer) {
            dialogueText.text = "Wrong answer! The enemy hits you!";
            DamagePopup.Create(playerUnit.transform.position + new Vector3(0, 1), "Hit", true);
            isDead = playerUnit.TakeDamage(enemyUnit.damage);
            playerHUD.SetHP(playerUnit);
        } else {
            dialogueText.text = "Correct answer! You take no damage!";
        }

        yield return new WaitForSeconds(2f);

        if(isDead) {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
        } else {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn());
        }
    }

    IEnumerator EndBattle() {
        if(state == BattleState.WON) {
            dialogueText.text = "You won the battle!";
        }
        if(state == BattleState.LOST) {
            dialogueText.text = "You were defeated.";
        }
        yield return new WaitForSeconds(2f);
        Destroy(GameObject.Find("BattleInfo"));
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator PlayerTurn() {
        string playerTurnText = "ATTACK TURN";
        dialogueText.text = playerTurnText;
        turnText.text = playerTurnText;
        yield return new WaitForSeconds(1f);
        PrepareAnswerPhase();
        changeActionButtonState(true);
    }

    IEnumerator PlayerAttack(int option) {
        bool isDead = false;
        if(option == correctAnswer) {
            isDead = enemyUnit.TakeDamage(playerUnit.damage);
            DamagePopup.Create(enemyUnit.transform.position, "Hit!", true);
            enemyHUD.SetHP(enemyUnit);
            dialogueText.text = "Correct answer!";
        } else {
            DamagePopup.Create(enemyUnit.transform.position, "Whack...", false);
            dialogueText.text = "Wrong answer!";
        }
        yield return new WaitForSeconds(2f);
        if(isDead) {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        } else {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    void PrepareAnswerPhase() {
        QuestionHandler questionObj = new QuestionHandler();
        last_pos = questionObj.SetInfo(last_pos);
        correctAnswer = questionObj.correct_answer;
        SetButtons(questionObj.answers);
        dialogueText.text = questionObj.question;
    }

    void SetButtons(string[] answers) {
        Button[] buttons = new Button[] { option1, option2, option3, option4 };
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].GetComponentInChildren<Text>().text = answers[i];
        }
    }


    //
    //
    //TRANSFORMAR NUM IF SÓ SEPARADO POR OR
    //
    //
    public void OnAttackButton(int option) {
        if(state == BattleState.PLAYERTURN) {
            changeActionButtonState(false);
            StartCoroutine(PlayerAttack(option));
        }
        if(state == BattleState.ENEMYTURN) {
            changeActionButtonState(false);
            StartCoroutine(EnemyAttack(option));
        } else {
            return;
        }
    }

    void changeActionButtonState(bool actionButtonState) {
        option1.interactable = actionButtonState;
        option2.interactable = actionButtonState;
        option3.interactable = actionButtonState;
        option4.interactable = actionButtonState;
    }
}

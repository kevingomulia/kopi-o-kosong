﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Events;

public class Customer : MonoBehaviour
{
	// Start is called before the first frame update
	public List<Drink> incomplete = new List<Drink>();
	public List<Drink> fulfilled = new List<Drink>();
	public float timeRemaining;
	public bool success;
    public bool isActiveCustomer;

    public TextMeshPro tmp;

    [SerializeField]
	UnityEvent onComplete;

	SpriteRenderer spriteRenderer;
	public Sprite[] characters = new Sprite[5];
	public void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		int index = Random.Range(0, 5);
		spriteRenderer.sprite = characters[index];
	}

	public void Init(Difficulty difficulty, float timeLimit)
	{
		incomplete = difficulty.GenerateDrinkList();
		timeRemaining = timeLimit;
        transform.GetComponent<SpriteRenderer>().sortingOrder = 999 - transform.GetSiblingIndex() * 3; // it's 5 am. fuck it.
    }

	public bool SubmitDrink(Drink completedDrink)
	{
		foreach (Drink drink in incomplete)
		{
			if (drink.Equals(completedDrink))
			{
				incomplete.Remove(drink);
				fulfilled.Add(drink);
				return true;
			}
		}
		return false;
	}

	public bool IsCompleted()
	{
		return (incomplete.Count == 0);
	}

	public void OnComplete()
	{
		onComplete.Invoke();
	}

	void Update()
	{
		timeRemaining -= Time.deltaTime;
	}

	public void OnFinishTween()
	{
		Destroy(gameObject);
	}

	// To remove all the text when auntie spawns
	public void ClearText() 
	{
		tmp.text = "";
	}

    public void RenderText()
    {
		// Do not render text when it is not the object in front
		if (transform.GetSiblingIndex() != 0)
			return;

        transform.GetComponent<SpriteRenderer>().sortingOrder = 999;
        tmp.sortingOrder = 999;

        List<string> incompleteDrinks = new List<string>();
        List<string> completeDrinks = new List<string>();


        foreach(Drink drink in incomplete)
        {
            incompleteDrinks.Add(drink.ToString());
        }
        string incompleteDrinksStr = "<b>" + string.Join(", ", incompleteDrinks.ToArray());
        incompleteDrinksStr += "</b>";

        foreach(Drink drink in fulfilled)
        {
            completeDrinks.Add(drink.ToString());
        }
        string completeDrinkStr = "<s>" + string.Join(", ", completeDrinks.ToArray());
        completeDrinkStr += "</s>";

        string text = "I would like a " + incompleteDrinksStr + completeDrinkStr;

        tmp.text = text + ".";
        tmp.GetComponentInParent<VertexJitter>().StartAnim();
    }

	public void ForceRenderText()
    {
        List<string> incompleteDrinks = new List<string>();
        List<string> completeDrinks = new List<string>();


        foreach(Drink drink in incomplete)
        {
            incompleteDrinks.Add(drink.ToString());
        }
        string incompleteDrinksStr = "<b>" + string.Join(", ", incompleteDrinks.ToArray());
        incompleteDrinksStr += "</b>";

        foreach(Drink drink in fulfilled)
        {
            completeDrinks.Add(drink.ToString());
        }
        string completeDrinkStr = "<s>" + string.Join(", ", completeDrinks.ToArray());
        completeDrinkStr += "</s>";

        string text = "I would like a " + incompleteDrinksStr + completeDrinkStr;

        tmp.text = text + ".";
		Debug.Log(tmp.text);
        tmp.GetComponentInParent<VertexJitter>().StartAnim();
    }
}
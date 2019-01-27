﻿using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;
using System.Linq;

public class Orderer : Singleton<Orderer>
{
    public List<Customer> orders = new List<Customer>();
    private Auntie auntie;

    public GameObject customerPrefab;
	public GameObject auntiePrefab;
    public GameObject finalDestination;
	public GameObject auntieDestination;
    public GameObject auntieShoutController;
    public GameObject gameController;
    private AudioSource audioSource { get { return GetComponent<AudioSource>(); } }
	public AudioClip correctAudioClip;
	public AudioClip incorrectAudioClip;

    private bool auntieDelay = false;
    private Vector3 offset = new Vector3(1f, 0f, 0f);


    // Start is called before the first frame update
    void Start()
    {
		gameObject.AddComponent<AudioSource>().volume = Settings.EffectsVolume;
		auntie = null;
        StartCoroutine(AuntieCoroutine());
        StartCoroutine(OrderCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
		int removed = orders.RemoveAll(c => c.timeRemaining < 0f);
		if (removed > 0)
		{
			//@todo: penalty
		}
	}

    IEnumerator AuntieCoroutine()
    {
        while (true)
        {
            Difficulty difficulty = gameController.GetComponent<GameController>().difficulty;
            Drink d = difficulty.GetDrink();
            if (auntie == null && auntieDelay) 
            {
                Debug.Log("Auntie Generated");
                var threshold = 0.5f * difficulty.stageDifficulty + 0.25f;
                if (Random.Range(0f, 1f) < threshold)
                {
                    auntieShoutController.GetComponent<AuntieShoutController>().SpawnAuntieShout(d.ToString());
                    yield return new WaitForSeconds(3);
                    GenerateAuntie(difficulty, d);
                }
            }

            auntieDelay = true;
            yield return new WaitForSeconds((5 - difficulty.stageDifficulty) * 3 + 10);
        }
    }

    IEnumerator OrderCoroutine()
    {
        while (true)
        {
            Difficulty difficulty = gameController.GetComponent<GameController>().difficulty;
            GenerateOrder(difficulty);
            yield return new WaitForSeconds((5 - difficulty.stageDifficulty)  + 6);
        }
    }

    public bool checkAndScoreDrink(Drink drink)
    {
		if (orders.Count == 0) {
			return false;
		}
		Customer active = orders[0];
        // Fill aunty's orders first!!!!
        if (auntie != null)
        {
			Debug.Log("Serve Auntie");
            if (auntie.SubmitDrink(drink))
            {
				Debug.Log("Serve Auntie CORRECT");
                // SCORE!
				playClip(correctAudioClip);
				gameController.GetComponent<GameController>().AddScore((int)Mathf.Floor(auntie.timeRemaining));
				auntie.angerLevel = 0;
				auntie.Leave();
                auntie = null;
                // Render unker's orders
                active.SetSpeechVisible(true);
				return true;
            }
            return false;
        }
        if (active.SubmitDrink(drink))
        {
			playClip(correctAudioClip);
            Difficulty difficulty = gameController.GetComponent<GameController>().difficulty;
            gameController.GetComponent<GameController>().AddScore((int)Mathf.Floor(active.timeRemaining));
            gameController.GetComponent<GameController>().AddTime(difficulty.stageDifficulty);
            active.angerLevel = 0;
            active.Leave();
            ProcessQueue();
            return true;
        }
		PlayIncorrectOrder(active);
        return false;
    }

	void PlayIncorrectOrder(Customer active) {
		audioSource.volume = 1;
        if (active.angerLevel >= 3) ProcessQueue();
        active.PlayOnIncorrect();
		playClip(incorrectAudioClip);
	}

	public void playClip(AudioClip audioClip)
	{
		audioSource.Stop();
		audioSource.clip = audioClip;
		audioSource.volume = Settings.EffectsVolume;
		audioSource.PlayOneShot(audioClip);
	}

    void GenerateOrder(Difficulty stageDifficulty)
    {
        Customer cust = Instantiate(customerPrefab, new Vector3(10f, 0f, 0f), Quaternion.identity, transform).GetComponent<Customer>();
        cust.Init(stageDifficulty, 60.0f);
        int positionInQueue = orders.Count();
        cust.SetLayerOrder(999 - positionInQueue);
        cust.MoveTo(finalDestination.transform.position + offset * positionInQueue);
        orders.Add(cust);
    }

    void ProcessQueue()
    {
        orders.RemoveAt(0);
        for (int i = 0; i < orders.Count(); i++)
        {
            Customer cust = orders[i];
            cust.MoveTo(finalDestination.transform.position + offset * i);
            cust.SetLayerOrder(999 - i);
        }
        if (orders.Any())
        {
            Customer activeCustomer = orders[0].GetComponent<Customer>();
            activeCustomer.ForceRenderText();
        }
    }

    void GenerateAuntie(Difficulty stageDifficulty, Drink drink)
    {
		// Generate auntie flying text

		// Coroutine
		// Generate auntie 3s later
		auntie = Instantiate(auntiePrefab, new Vector3(10f, 0f, 0f), Quaternion.identity, transform).GetComponent<Auntie>();
		auntie.Init(stageDifficulty, Random.Range(5,10), drink);

		auntie.MoveTo(auntieDestination.transform.position);

		// Auntie is priority
		if (orders.Count > 0) orders[0].SetSpeech(false);
		
    }
}

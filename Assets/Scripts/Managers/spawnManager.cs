using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnManager : MonoBehaviour
{
    [Header("----- Components -----")]
    [SerializeField] List<GameObject> spawnList = new List<GameObject>();
    [SerializeField] List<GameObject> eliteSpawnList = new List<GameObject>();
    [SerializeField] List<GameObject> enemies = new List<GameObject>();
    [SerializeField] List<GameObject> elites = new List<GameObject>();
    [SerializeField] GameObject phantom;
    [SerializeField] List<GameObject> eliteTypes;
    public static spawnManager instance;

    [Header("----- Wave stats -----")]
    [SerializeField] int wave;
    [SerializeField] int enemyLimit;
    [SerializeField] int maxLimit;
    [SerializeField] float spawnRate;
    [SerializeField] int atTheSameTime;
    [SerializeField] int eliteLimit;
    [SerializeField] int maxElites;
    [SerializeField] float eliteRate;
    [SerializeField] int waveLength;

    int enemiesInScene;
    int elitesInScene;
    bool spawning;
    bool eliteSpawn;
    bool firstSpawn;
    public bool inWave;

    void Start()
    {
        instance = this;
        GameObject[] locations = GameObject.FindGameObjectsWithTag("SpawnPos");
        GameObject[] eliteLocals = GameObject.FindGameObjectsWithTag("ElitePos");

        for (int i = 0; i < locations.Length; i++)
        {
            spawnList.Add(locations[i]);
        }

        for (int i = 0; i < eliteLocals.Length; i++)
        {
            eliteSpawnList.Add(eliteLocals[i]);
        }

        startWave();
    }

    void Update()
    {
        if (inWave)
        {
            if (enemiesInScene < enemyLimit && !spawning)
                StartCoroutine(spawnMore());

            if (elitesInScene < eliteLimit && !eliteSpawn && wave != 1)
                StartCoroutine(spawnElite());
        }
    }

    public void startWave()
    {
        //Increment wave
        wave++;
        firstSpawn = true;

        gameManager.instance.waveText.text = "Wave: " + wave;
        gameManager.instance.anim.SetTrigger("NewWave");

        //Increase difficulty
        if (wave % 3 == 0)
        {
            if (spawnRate > 1)
                spawnRate -= .5f;

            if (atTheSameTime < 5)
                atTheSameTime++;

            if (eliteRate > 3)
                eliteRate -= 0.5f;

            if (eliteLimit < maxElites)
                eliteLimit++;
        }

        if (wave != 1 && enemyLimit < maxLimit)
            enemyLimit += 2;

        //Spawn an enemy at every position
        for (int i = 0; i < spawnList.Count; i++)
        {
            spawnEnemy(i);
        }

        //Start wave timer
        StartCoroutine(waveTimer());
    }

    IEnumerator spawnMore()
    {
        spawning = true;
        int spawned = 0;

        while (spawned < atTheSameTime && !firstSpawn)
        {
            spawnEnemy(Random.Range(0, spawnList.Count));
            spawned++;
        }

        firstSpawn = false;

        yield return new WaitForSeconds(spawnRate);
        spawning = false;
    }

    IEnumerator spawnElite()
    {
        eliteSpawn = true;
        bool temp = false;

        elitesInScene++;
        GameObject local = eliteSpawnList[Random.Range(0, eliteSpawnList.Count)];
        GameObject spawned = Instantiate(eliteTypes[Random.Range(0, eliteTypes.Count)], local.transform.position, local.transform.rotation);

        for (int i = 0; i < elites.Count; i++)
        {
            if (elites[i] == null)
            {
                elites[i] = spawned;
                temp = true;
                break;
            }
        }

        if (!temp)
            elites.Add(spawned);

        yield return new WaitForSeconds(eliteRate);
        eliteSpawn = false;
    }

    public void enemyDeath()
    {
        enemiesInScene--;
    }

    void spawnEnemy(int location)
    {
        if(enemiesInScene < enemyLimit)
        {
            enemiesInScene++;
            bool temp = false;
            GameObject spawned = Instantiate(phantom, spawnList[location].transform.position, spawnList[location].transform.rotation);
            
            for(int i = 0; i < enemies.Count; i++)
            {
                if(enemies[i] == null)
                {
                    enemies[i] = spawned;
                    temp = true;
                    break;
                }
            }

            if(!temp)
                enemies.Add(spawned);
        }
    }

    IEnumerator waveTimer()
    {
        inWave = true;
        gameManager.instance.nextWaveText.active = false;

        yield return new WaitForSeconds(waveLength);

        gameManager.instance.nextWaveText.active = true;
        inWave = false;

        for (int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i] != null)
                enemies[i].GetComponent<enemyBase>().death();
        }

        for (int i = 0; i < elites.Count; i++)
        {
            if (elites[i] != null)
                elites[i].GetComponent<enemyBase>().death();
        }
    }
}

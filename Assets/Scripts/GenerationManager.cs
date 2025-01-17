﻿using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerationManager : MonoBehaviour
{
    [Header("Generators")]
    [SerializeField]
    private GenerateObjectsInArea[] boxGenerators;
    [SerializeField]
    private GenerateObjectsInArea boatGenerator;
    [SerializeField]
    private GenerateObjectsInArea pirateGenerator;

    [Space(10)]
    [Header("Parenting and Mutation")]
    [SerializeField] 
    private bool sexualReproduction;
    [SerializeField] [Range(0, 100)]
    private int crossoverChance;
    [SerializeField] [Range(0, 100)]
    private int extraMutationChance;
    [Space(10)]
    [SerializeField]
    private float mutationFactor;
    [SerializeField] 
    private float mutationChance;
    [SerializeField] 
    private int boatParentSize;
    [SerializeField] 
    private int pirateParentSize;

    [Space(10)] 
    [Header("Simulation Controls")]
    [SerializeField, Tooltip("Time per simulation (in seconds).")]
    private float simulationTimer;
    [SerializeField, Tooltip("Current time spent on this simulation.")]
    private float simulationCount;
    [SerializeField, Tooltip("Automatically starts the simulation on Play.")]
    private bool runOnStart;
    [SerializeField, Tooltip("Initial count for the simulation. Used for the Prefabs naming.")]
    private int generationCount;

    [Space(10)] 
    [Header("Prefab Saving")]
    [SerializeField]
    private string savePrefabsAt;
    
    /// <summary>
    /// Those variables are used mostly for debugging in the inspector.
    /// </summary>
    [Header("Former winners")]
    [SerializeField]
    private AgentData lastBoatWinnerData;
    [SerializeField]
    private AgentData lastPirateWinnerData;

    private bool _runningSimulation;
    private List<BoatLogic> _activeBoats;
    private List<PirateLogic> _activePirates;
    private BoatLogic[] _boatParents;
    private PirateLogic[] _pirateParents;

    private StreamWriter writer;


    private void Awake()
    {
        Random.InitState(6);
        ResetOutputs();
    }

    private void Start()
    {
        if (runOnStart)
        {
            StartSimulation();
        }
    }
    
    private void Update()
    {
        if (_runningSimulation)
        {
            //Creates a new generation.
            if (simulationCount >= simulationTimer)
            {
                ++generationCount;
                MakeNewGeneration();
                simulationCount = -Time.deltaTime;
            } 
            simulationCount += Time.deltaTime;
        }
    }

     
    /// <summary>
    /// Generates the boxes on all box areas.
    /// </summary>
    public void GenerateBoxes()
    {
        foreach (GenerateObjectsInArea generateObjectsInArea in boxGenerators)
        {
            generateObjectsInArea.RegenerateObjects();
        }
    }
    
     /// <summary>
     /// Generates boats and pirates using the parents list.
     /// If no parents are used, then they are ignored and the boats/pirates are generated using the default prefab
     /// specified in their areas.
     /// </summary>
     /// <param name="boatParents"></param>
     /// <param name="pirateParents"></param>
    public void GenerateObjects(BoatLogic[] boatParents = null, PirateLogic[] pirateParents = null)
    {
        GenerateBoats(boatParents);
        GeneratePirates(pirateParents);
    }

     /// <summary>
     /// Generates the list of pirates using the parents list. The parent list can be null and, if so, it will be ignored.
     /// Newly created pirates will go under mutation (MutationChances and MutationFactor will be applied).
     /// Newly create agents will be Awaken (calling AwakeUp()).
     /// </summary>
     /// <param name="pirateParents"></param>
    private void GeneratePirates(PirateLogic[] pirateParents)
    {
        _activePirates = new List<PirateLogic>();
        List<GameObject> objects = pirateGenerator.RegenerateObjects();
        foreach (GameObject obj in objects)
        {
            PirateLogic pirate = obj.GetComponent<PirateLogic>();
            if (pirate != null)
            {
                _activePirates.Add(pirate);
                if (pirateParents != null)
                {
                    if (sexualReproduction && pirateParents.Length > 1){
                        PirateLogic pirateParent1 = pirateParents[Random.Range(0, pirateParents.Length)];
                        PirateLogic pirateParent2 = pirateParents[Random.Range(0, pirateParents.Length)];;
                        while (pirateParent1 != pirateParent2){
                            pirateParent2 = pirateParents[Random.Range(0, pirateParents.Length)];
                        }

                        pirate.Birth(pirateParent1.GetData(), pirateParent2.GetData(), crossoverChance);
                    }
                    else{
                        PirateLogic pirateParent = pirateParents[Random.Range(0, pirateParents.Length)];
                        pirate.Birth(pirateParent.GetData());
                    }
                }

                //Do an extra mutation if sexual reproduction
                if (sexualReproduction) pirate.Mutate(mutationFactor, extraMutationChance); 
                else pirate.Mutate(mutationFactor, mutationChance);
                pirate.AwakeUp();
            }
        }
    }


     /// <summary>
     /// Generates the list of boats using the parents list. The parent list can be null and, if so, it will be ignored.
     /// Newly created boats will go under mutation (MutationChances and MutationFactor will be applied).
     /// /// Newly create agents will be Awaken (calling AwakeUp()).
     /// </summary>
     /// <param name="boatParents"></param>
    private void GenerateBoats(BoatLogic[] boatParents)
    {
        _activeBoats = new List<BoatLogic>();
        List<GameObject> objects = boatGenerator.RegenerateObjects();
        foreach (GameObject obj in objects)
        {
            BoatLogic boat = obj.GetComponent<BoatLogic>();
            if (boat != null)
            {
                _activeBoats.Add(boat);
                if (boatParents != null)
                {
                    if (sexualReproduction && boatParents.Length > 1){
                        BoatLogic boatParent1 = boatParents[Random.Range(0, boatParents.Length)];
                        BoatLogic boatParent2 = boatParents[Random.Range(0, boatParents.Length)];;
                        while (boatParent1 != boatParent2){
                            boatParent1 = boatParents[Random.Range(0, boatParents.Length)];
                        }

                        boat.Birth(boatParent1.GetData(), boatParent2.GetData(), crossoverChance);
                    }
                    else {
                        BoatLogic boatParent = boatParents[Random.Range(0, boatParents.Length)];
                        boat.Birth(boatParent.GetData());
                    }
                }
                if (sexualReproduction) boat.Mutate(mutationFactor, extraMutationChance); 
                else boat.Mutate(mutationFactor, mutationChance);
                boat.AwakeUp();
            }
        }
    }

     /// <summary>
     /// Creates a new generation by using GenerateBoxes and GenerateBoats/Pirates.
     /// Previous generations will be removed and the best parents will be selected and used to create the new generation.
     /// The best parents (top 1) of the generation will be stored as a Prefab in the [savePrefabsAt] folder. Their name
     /// will use the [generationCount] as an identifier.
     /// </summary>
    public void MakeNewGeneration()
    {
        Random.InitState(6);

        GenerateBoxes();
        
        //Remove all null elements and sort the active boats (O(n log n))
        _activeBoats.RemoveAll(item => item == null);
        _activeBoats.Sort();
        if (_activeBoats.Count == 0)
        {
            GenerateBoats(_boatParents);
        }
        _boatParents = new BoatLogic[boatParentSize];

        //This generation will be the new parents
        for (int i = 0; i < boatParentSize; i++) 
        {
            _boatParents[i] = _activeBoats[i];
        }

        //And save the best boat
        BoatLogic lastBoatWinner = _activeBoats[0];
        lastBoatWinner.name += "Gen-" + generationCount; 
        lastBoatWinnerData = lastBoatWinner.GetData();
        PrefabUtility.SaveAsPrefabAsset(lastBoatWinner.gameObject, savePrefabsAt + lastBoatWinner.name + ".prefab");
        
        _activePirates.RemoveAll(item => item == null);
        _activePirates.Sort();
        _pirateParents = new PirateLogic[pirateParentSize];
        for (int i = 0; i < pirateParentSize; i++)
        {
            _pirateParents[i] = _activePirates[i];
        }

        PirateLogic lastPirateWinner = _activePirates[0];
        lastPirateWinner.name += "Gen-" + generationCount; 
        lastPirateWinnerData = lastPirateWinner.GetData();
        PrefabUtility.SaveAsPrefabAsset(lastPirateWinner.gameObject, savePrefabsAt + lastPirateWinner.name + ".prefab");
        
        //Winners:
        Debug.Log("Last winner boat had: " + lastBoatWinner.GetPoints() + " points!" + " Last winner pirate had: " + lastPirateWinner.GetPoints() + " points!");
        OutputText(lastBoatWinner);
        OutputText(lastPirateWinner);

        GenerateObjects(_boatParents, _pirateParents);
    }

     /// <summary>
     /// Starts a new simulation. It does not call MakeNewGeneration. It calls both GenerateBoxes and GenerateObjects and
     /// then sets the _runningSimulation flag to true.
     /// </summary>
    public void StartSimulation()
    {
        Random.InitState(6);

        GenerateBoxes();
        GenerateObjects();
        _runningSimulation = true;
    }

     /// <summary>
     /// Continues the simulation. It calls MakeNewGeneration to use the previous state of the simulation and continue it.
     /// It sets the _runningSimulation flag to true.
     /// </summary>
     public void ContinueSimulation()
     {
         MakeNewGeneration();
         _runningSimulation = true;
     }
     
     /// <summary>
     /// Stops the count for the simulation. It also removes null (Destroyed) boats from the _activeBoats list and sets
     /// all boats and pirates to Sleep.
     /// </summary>
    public void StopSimulation()
    {
        _runningSimulation = false;
        _activeBoats.RemoveAll(item => item == null);
        _activeBoats.ForEach(boat => boat.Sleep());
        _activePirates.ForEach(pirate => pirate.Sleep());
    }

    private void OutputText(AgentLogic agentWinner){

        if (agentWinner is BoatLogic) writer = File.AppendText(Directory.GetCurrentDirectory() + @"\boats.csv");
        else writer = File.AppendText(Directory.GetCurrentDirectory() + @"\pirates.csv");

        float points = agentWinner.GetPoints();

        //For some culture reasons, writeLine writes floats with coma instead of a dot
        NumberFormatInfo numberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator=".",
        };
        writer.WriteLine(points.ToString("0.00", numberFormat));
        writer.Close();
    }

    private void ResetOutputs(){

        if (File.Exists((Directory.GetCurrentDirectory() + @"\boats.csv")))
            File.Delete((Directory.GetCurrentDirectory() + @"\boats.csv"));
        if (File.Exists((Directory.GetCurrentDirectory() + @"\pirates.csv")))
            File.Delete((Directory.GetCurrentDirectory() + @"\pirates.csv"));
        
        writer = File.AppendText(Directory.GetCurrentDirectory() + @"\boats.csv");
        writer.WriteLine("Points");
        writer.Close();

        writer = File.AppendText(Directory.GetCurrentDirectory() + @"\pirates.csv");
        writer.WriteLine("Points");
        writer.Close();
    }
}

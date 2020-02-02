using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class World : MonoBehaviour
{
    public Scenario scenario;

    // Start is called before the first frame update
    void Start()
    {
        scenario = ScriptableObject.CreateInstance<Scenario>();
        scenario.Start();
    }

    // Update is called once per frame
    void Update()
    {
        scenario.Update();
    }

    void FixedUpdate()
    {
        scenario.FixedUpdate();
    }
}

public class Scenario : ScriptableObject
{

    public Material grass;
    public Material forest;
    public Material water;
    public Material fire;
    public Material dead;

    public Hexsphere world;

    public int maximumIncidents;

    public Func<float, float, float, float> easing;

    public readonly Dictionary<Vehicle, int> availableVehicles = new Dictionary<Vehicle, int>();

    public readonly Dictionary<Biome, int> availableBiomes = new Dictionary<Biome, int>();

    public readonly Dictionary<Incident, int> potentialIncidents = new Dictionary<Incident, int> {
        { new ForestFire(), 1 }
    };

    public readonly List<Biome> biomes = new List<Biome>();

    public readonly List<ActiveIncident> activeIncidents = new List<ActiveIncident>();

    public Player player = new Player();

    public void Start()
    {

        world = (Hexsphere)GameObject.Find("Hexsphere").GetComponent<Hexsphere>();

        grass = (Material)Resources.Load("Materials/Grass", typeof(Material));
        forest = (Material)Resources.Load("Materials/Forest", typeof(Material));
        water = (Material)Resources.Load("Materials/Water", typeof(Material));
        fire = (Material)Resources.Load("Materials/Fire", typeof(Material));
        dead = (Material)Resources.Load("Materials/Dead", typeof(Material));

        // TODO: This should be based on the intensity of the scenario.
        // randomly place x number of each biome listed in availableBiomes
        for (var index = 0; index < world.TileCount; ++index)
        {
            Biome biome;
            if ((index % 3) == 0)
            {
                biome = new Sea(world.tiles[index], water);
            }
            else if ((index % 3) == 1)
            {
                biome = new Forest(world.tiles[index], forest, fire, dead);
            }
            else
            {
                biome = new Plain(world.tiles[index], grass, fire, dead);
            }
            biomes.Add(biome);
        }

        foreach (var biome in biomes)
        {
            biome.Apply();
        }

    }

    public void Update()
    {

        // handle keyboard / vr input

        //      toggle select vehicle
        //      toggle select biome (tile)
        //      dispatch vehicle to biome

        //      stretch goal:
        //          toggle hud
        //          trigger action on hud
        //              select dock
        //              select vehicle
        //              purchase (if available funds match or exceed cost)

        foreach (var biome in biomes)
        {
            if (biome.Changed)
            {
                biome.Apply();
            }
        }

    }

    private int last = 0;

    public void FixedUpdate()
    {

        var currentTime = Time.time;

        // update state of action buttons based on selected objects

        //      if both biome and vehicle selected, enable dispatch button
        //      otherwise, disable dispatch button

        //      stretch goal:
        //          if both a dock and an available vehicle is selected and cost <= available funds, enable purchase button
        //          otherwise, diable purchase button

        // update position of movable objects
        foreach (var vehicle in player.Vehicles)
        {
            vehicle.Update(currentTime);
        }

        // trigger vehicle action

        //      if vehicle location equals destination location, execute action

        foreach (var activeIncident in activeIncidents)
        {
            activeIncident.Update(currentTime);
        }

        // TODO: calculate whether or not an incident begins

        if (currentTime < biomes.Count)
        {

            var biomeIndex = (int)(currentTime % biomes.Count);
            var incidentIndex = (int)(currentTime % potentialIncidents.Keys.Count);

            if (biomeIndex > last)
            {
                Debug.Log($"Creating active incident {biomeIndex} :: {incidentIndex}.");

                var biome = biomes[biomeIndex];
                var incident = potentialIncidents.Keys.ElementAt(incidentIndex);
                var activeIncident = new ActiveIncident(biome, incident);
                activeIncidents.Add(activeIncident);

                last = biomeIndex;
            }
        }

    }

}

public abstract class Biome
{

    private int damage;

    private bool changed;

    protected Tile tile;

    protected int strength;

    public abstract string Name { get; }

    public bool Changed => changed;

    protected int Damage => damage;

    public Biome(Tile tile)
    {
        this.tile = tile;
    }

    public abstract Dictionary<string, int> ResourcesProvided { get; }

    public abstract Dictionary<string, int> ResourcesNeeded { get; }

    public ActiveIncident activeIncident;

    public void Update(int damage)
    {

        if (this.damage != damage)
        {
            this.damage = damage;
            changed = true;
        }

    }

    public abstract void Apply();

}

public sealed class Sea : Biome
{

    private readonly Material water;

    public static string MyName = "Sea";

    public override string Name => MyName;

    public Sea(Tile tile, Material water)
        : base(tile)
    {
        this.water = water;
    }

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Water.MyName, 2 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int>
    {
    };

    public override void Apply()
    {

        tile.SetMaterial(water);

    }

}

public sealed class Forest : Biome
{

    private readonly Material forest;
    private readonly Material fire;
    private readonly Material dead;

    public static string MyName = "Forest";

    public override string Name => MyName;

    public Forest(Tile tile, Material forest, Material fire, Material dead)
        : base(tile)
    {
        this.forest = forest;
        this.fire = fire;
        this.dead = dead;
    }

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Seed.MyName, 3 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int> {
        { Seed.MyName, 5 },
        { Water.MyName, 3 }
    };

    public override void Apply()
    {

        var damage = Damage;

        if (damage < 1)
        {
            tile.SetMaterial(forest);
        }
        else if (damage < 100)
        {
            tile.SetMaterial(fire);
        }
        else
        {
            tile.SetMaterial(dead);
        }

    }

}

public sealed class Plain : Biome
{

    private readonly Material grass;
    private readonly Material fire;
    private readonly Material dead;

    public static string MyName = "Plain";

    public override string Name => MyName;

    public Plain(Tile tile, Material grass, Material fire, Material dead)
        : base(tile)
    {
        this.grass = grass;
        this.fire = fire;
        this.dead = dead;
    }

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Seed.MyName, 5 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int> {
        { Seed.MyName, 5 },
        { Water.MyName, 3 }
    };

    public override void Apply()
    {

        var damage = Damage;

        if (damage < 1)
        {
            tile.SetMaterial(grass);
        }
        else if (damage < 100)
        {
            tile.SetMaterial(fire);
        }
        else
        {
            tile.SetMaterial(dead);
        }

    }

}

public abstract class Vehicle
{

    protected Vector3 currentLocation;

    protected Vector3 beginLocation;

    protected Vector3 endLocation;

    protected float beginTime;

    public abstract int Capacity { get; }

    public void Service(Biome biome)
    {

        // update begin and end

    }

    public void Update(float currentTime)
    {

        // if the current location does not equal the end location
        //      do the lerp

    }

    public void Apply(GameObject vehicle)
    {

        // update the current location to the vehicle

    }

}

public sealed class Ship : Vehicle
{

    public override int Capacity => 500;
}

public abstract class Incident
{

    public int Severity { get; }

    public abstract List<string> AppliesTo { get; }

    public abstract string Name { get; }

}

public sealed class ForestFire : Incident
{

    public static string MyName = "Forest Fire";

    public override List<string> AppliesTo => new List<string> {
        Forest.MyName
    };

    public override string Name => ForestFire.MyName;

}

public sealed class Drought : Incident
{

    public static string MyName = "Drought";

    public override List<string> AppliesTo => new List<string> {
        Plain.MyName
    };

    public override string Name => Drought.MyName;

}

public sealed class ActiveIncident
{

    public Biome biome;

    public Incident incident;

    public int intensity;

    public ActiveIncident(Biome biome, Incident incident)
    {
        this.biome = biome;
        this.incident = incident;
    }

    // TODO: Fix this (should cause damage based on some time scale?)
    public void Update(float currentTime)
    {
        this.biome.Update((int)(currentTime));
    }

}

public abstract class Resource
{

    public abstract string Name { get; }

    public abstract int Weight { get; }

}

public sealed class Seed : Resource
{

    public static string MyName = "Seed";

    public override string Name => Seed.MyName;

    public override int Weight => 100;

}

public sealed class Water : Resource
{

    public static string MyName = "Water";

    public override string Name => MyName;

    public override int Weight => 250;

}

public sealed class Player
{

    private readonly List<Vehicle> vehicles = new List<Vehicle>();

    public List<Vehicle> Vehicles => vehicles;

    public Vehicle selectedVehicle;

    public Biome selectedBiome;

}
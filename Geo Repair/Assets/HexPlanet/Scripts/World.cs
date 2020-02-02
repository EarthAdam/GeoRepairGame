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

        // Enable this for keyboard control:
        // float horizontalValue = Input.GetAxis("Horizontal");
        // float verticalValue = Input.GetAxis("Vertical");
        
        // if (horizontalValue != 0)
        // {
        //     const float movementSpeed = 50f;
        //     var world = GameObject.Find("Hexsphere").GetComponent<Hexsphere>();
        //     world.transform.Rotate(0, horizontalValue * movementSpeed * Time.deltaTime, 0);
        // }
        
        // if (verticalValue != 0)
        // {
        //     const float movementSpeed = 1f;
        //     var player = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
        //     var next = player.transform.position + new Vector3(0, 0, verticalValue * movementSpeed * Time.deltaTime);
        //     if (next.z <= -2.5 && next.z >= -5) {
        //         player.transform.position = next;
        //     }
        // }

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
        { new ForestFire(), 15 }
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

        availableBiomes.Add(new Forest(null, forest, fire, dead), 48);
        availableBiomes.Add(new Plain(null, grass, fire, dead), 25);

        var spaces = new Biome[world.TileCount];

        var sea = new Sea(null, water);
        for (var index = 0; index < world.TileCount; ++index)
        {
            spaces[index] = sea;
        }

        var position = 0;
        foreach (var availableBiome in availableBiomes)
        {
            for (var index = 0; index < availableBiome.Value; ++index)
            {
                spaces[position++] = availableBiome.Key;
            }
        }

        for (var index = 0; index < world.TileCount; ++index)
        {
            var swap = (int)UnityEngine.Random.Range(0, world.TileCount - 2);
            if (index != swap)
            {
                var biome = spaces[index];
                spaces[index] = spaces[swap];
                spaces[swap] = biome;
            }
        }

        for (var index = 0; index < world.TileCount; ++index)
        {
            biomes.Add(spaces[index].Clone(world.tiles[index]));
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

        var exhausted = new List<ActiveIncident>();
        foreach (var activeIncident in activeIncidents)
        {
            if (activeIncident.Update(currentTime) == false)
            {
                exhausted.Add(activeIncident);
            };
        }
        foreach (var remove in exhausted)
        {
            activeIncidents.Remove(remove);
        }

        // TODO: (maximum number of concurrent incidents - number of active instances) * result of the Lerp function for the scenario <- (time / 120 = year)
        var maximumIncidents = 0;
        foreach (var potentialIncident in potentialIncidents)
        {
            maximumIncidents += potentialIncident.Value;
        }

        var currentIncidents = activeIncidents.Count;

        if (currentIncidents < maximumIncidents)
        {
            var likelihood = 10;
            if (UnityEngine.Random.Range(1, 210) < likelihood)
            {
                var healthy = new List<Biome>();
                foreach (var biome in biomes)
                {
                    if (biome.ActiveIncident == null)
                    {
                        healthy.Add(biome);
                    }
                }
                var biomeIndex = UnityEngine.Random.Range(0, healthy.Count - 1);
                var incidentIndex = (int)(currentTime % potentialIncidents.Keys.Count);

                Debug.Log($"Creating active incident {biomeIndex} :: {incidentIndex}.");

                {
                    var biome = healthy[biomeIndex];
                    var incident = potentialIncidents.Keys.ElementAt(incidentIndex);
                    var activeIncident = biome.Activate(incident, currentTime);
                    activeIncidents.Add(activeIncident);
                }
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

    private ActiveIncident activeIncident;

    public ActiveIncident ActiveIncident => activeIncident;

    public void Update(int damage)
    {

        if (this.damage != damage)
        {
            this.damage = damage;
            changed = true;
        }

        Debug.Log($"Setting damage to {damage}");

    }

    public ActiveIncident Activate(Incident incident, float currentTime)
    {
        activeIncident = new ActiveIncident(this, incident, currentTime);
        return activeIncident;
    }

    public abstract void Apply();

    public abstract Biome Clone(Tile tile);

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

    public override Biome Clone(Tile tile)
    {
        return new Sea(tile, water);
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

    public override Biome Clone(Tile tile)
    {
        return new Forest(tile, forest, fire, dead);
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

    public override Biome Clone(Tile tile)
    {
        return new Plain(tile, grass, fire, dead);
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

    public float startTime;

    public ActiveIncident(Biome biome, Incident incident, float currentTime)
    {
        this.biome = biome;
        this.incident = incident;
        this.startTime = currentTime;
    }

    // TODO: Fix this (should cause damage based on some time scale?)
    public bool Update(float currentTime)
    {
        var damage = (int)(currentTime - startTime) * 4;

        this.biome.Update(damage);

        if (damage >= 100)
        {
            return false;
        }
        else
        {
            return true;
        }
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

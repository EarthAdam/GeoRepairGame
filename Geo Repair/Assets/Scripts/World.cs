﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class World : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Scenario : MonoBehaviour {

    public Hexsphere world;

    public int maximumIncidents;

    public Func<float, float, float, float> easing;

    public readonly Dictionary<Vehicle, int> availableVehicles = new Dictionary<Vehicle, int>();

    public readonly Dictionary<Biome, int> availableBiomes = new Dictionary<Biome, int>();

    public readonly Dictionary<Incident, int> potentialIncidents = new Dictionary<Incident, int>();

    public readonly List<Biome> biomes = new List<Biome>();

    public Player player;

    void Start() {

        // determine the size of the world

        // randomly place x number of each biome listed in availableBiomes

        // update world (materials for each tile)

    }

    void Update() {

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

        // update world (materials for biomes that changed state)
        foreach (var tuple in biomes.Zip(world.tiles, (biome, tile) => (biome, tile))) {
            tuple.biome.Apply(tuple.tile);
        }

    }

    void FixedUpdate() {

        var currentTime = Time.time;

        // update state of action buttons based on selected objects

        //      if both biome and vehicle selected, enable dispatch button
        //      otherwise, disable dispatch button

        //      stretch goal:
        //          if both a dock and an available vehicle is selected and cost <= available funds, enable purchase button
        //          otherwise, diable purchase button

        // update position of movable objects
        foreach (var vehicle in player.Vehicles) {
            vehicle.Update(currentTime);
        }

        // trigger vehicle action

        //      if vehicle location equals destination location, execute action

        // increment damage caused by active incidents

        foreach (var biome in biomes) {
            biome.Update(currentTime);
        }

        // calculate whether or not an incident begins
        
    }

}

public abstract class Biome {

    protected Tile tile;

    protected int strength;

    protected int damage;

    public abstract string Name { get; }

    public abstract Dictionary<string, int> ResourcesProvided { get; }

    public abstract Dictionary<string, int> ResourcesNeeded { get; }

    public ActiveIncident activeIncident;

    public void Update(float currentTime) {

        // if there is active incident
        //      call update on the active incident passing the current time

    }

    public void Apply(Tile tile) {

        // update the material of the tile to represent the current state

    }

}

public sealed class Sea : Biome {

    public static string MyName = "Sea";

    public override string Name => MyName;

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Water.MyName, 2 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int> {
    };

}

public sealed class Forest : Biome {

    public static string MyName = "Forest";

    public override string Name => MyName;

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Seed.MyName, 3 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int> {
        { Seed.MyName, 5 },
        { Water.MyName, 3 }
    };

}

public sealed class Plain : Biome {

    public static string MyName = "Plain";

    public override string Name => MyName;

    public override Dictionary<string, int> ResourcesProvided => new Dictionary<string, int> {
        { Seed.MyName, 5 }
    };

    public override Dictionary<string, int> ResourcesNeeded => new Dictionary<string, int> {
        { Seed.MyName, 5 },
        { Water.MyName, 3 }
    };

}

public abstract class Vehicle {

    protected Vector3 currentLocation;

    protected Vector3 beginLocation;

    protected Vector3 endLocation;

    protected float beginTime;

    public abstract int Capacity { get; }

    public void Service(Biome biome) {

        // update begin and end

    }

    public void Update(float currentTime) {

        // if the current location does not equal the end location
        //      do the lerp

    }

    public void Apply(GameObject vehicle) {

        // update the current location to the vehicle

    }

}

public sealed class Ship : Vehicle {

    public override int Capacity => 500;
}

public abstract class Incident {

    public int Severity { get; }

    public abstract List<string> AppliesTo { get; }

    public abstract string Name { get; }

}

public sealed class ForestFire : Incident {

    public static string MyName = "Forest Fire";

    public override List<string> AppliesTo => new List<string> {
        Forest.MyName
    };

    public override string Name => ForestFire.MyName;

}

public sealed class Drought : Incident {

    public static string MyName = "Drought";

    public override List<string> AppliesTo => new List<string> {
        Plain.MyName
    };

    public override string Name => Drought.MyName;

}

public sealed class ActiveIncident {

    public Biome biome;

    public Incident incident;

    public int intensity;

}

public abstract class Resource {

    public abstract string Name { get; }

    public abstract int Weight { get; }

}

public sealed class Seed : Resource {

    public static string MyName = "Seed";

    public override string Name => Seed.MyName;

    public override int Weight => 100;

}

public sealed class Water : Resource {

    public static string MyName = "Water";

    public override string Name => MyName;

    public override int Weight => 250;

}

public sealed class Player {

    private readonly List<Vehicle> vehicles = new List<Vehicle>();

    public List<Vehicle> Vehicles => vehicles;

    public Vehicle selectedVehicle;
    
    public Biome selectedBiome;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed;
    public Vector3 offset;

    public Tilemap tilemap;

    public Vector2 minBounds; // Bottom left corner of the map
    public Vector2 maxBounds; // Top right corner of the map

    private void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;

        Vector3 min = tilemap.CellToWorld(bounds.min); // Gets the world position of the bottom left corner of the tilemap
        Vector3 max = tilemap.CellToWorld(bounds.max); // Gets the world position of the top right corner of the tilemap

        minBounds = new Vector2(min.x, min.y); 
        maxBounds = new Vector2(max.x, max.y);
    }
    private void Update()
    {
        Vector3 pos = Vector3.Lerp(transform.position, player.position + offset, moveSpeed * Time.deltaTime); // Smooths movement

        pos.x = Mathf.Clamp(pos.x, minBounds.x + 9f, maxBounds.x - 9f); // Stops the camera going out of bounds (Plus and minus 12 for camera x size)
        pos.y = Mathf.Clamp(pos.y, minBounds.y + 5f, maxBounds.y - 5f); // Plus and minus 5 for camera y size

        pos.z = -10f; // Fixes z
        transform.position = pos;
    }

}
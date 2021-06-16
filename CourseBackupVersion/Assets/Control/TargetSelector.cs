using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for keeping track of game objects to be targeted by enemies.
public class TargetSelector
{
    private GameObject[] targets = new GameObject[64];
    private int arraySize = 64;
    private int quantity = 0;

    public bool enemiesAreDestroyed = false;
    public bool bossIsDestroyed = false; // to be set externally by the boss itself

    public TargetSelector(int maximumSize)
    {
        arraySize = maximumSize;
        targets = new GameObject[maximumSize];
    }

    public void FlagEnemiesAsDestroyed()
    {
        enemiesAreDestroyed = true;

        if (bossIsDestroyed)
        {
            GameManager.Instance.Halt();
        }
    }

    public void FlagBossAsDestroyed()
    {
        bossIsDestroyed = true;

        if (enemiesAreDestroyed)
        {
            GameManager.Instance.Halt();
        }
    }

    public void Insert(GameObject newTarget)
    {
        if (newTarget == null)
        {
            return;
        }

        if (quantity < arraySize)
        {
            targets[quantity] = newTarget;
            quantity++;
        }
    }

    private void RemoveByIndex(int index)
    {
        if (0 <= index && index < quantity)
        {
            targets[index] = null;
            quantity--;

            while (index < quantity)
            {
                targets[index] = targets[index + 1];
                index++;
                targets[index] = null;
            }
        }

        if (quantity < 2)
        {
            FlagEnemiesAsDestroyed();
        }
    }

    public void RemoveBySearch(GameObject target)
    {
        if (target != null)
        {
            for (int index = 0; index < quantity; index++)
            {
                if (ReferenceEquals(targets[index], target))
                {
                    RemoveByIndex(index);
                }
            }
        }
    }
    public GameObject GetRandomTarget()
    {
        GameObject selection = null;
        while (selection == null && quantity > 0)
        {
            int index = Random.Range(0, quantity);
            selection = targets[index];
            if (selection == null)
            {
                RemoveByIndex(index);
            }
        }
        return selection;
    }
    public GameObject GetRandomTargetExcept(GameObject excluding)
    {
        GameObject selection = null;
        while (selection == null && quantity > 1)
        {
            int index = Random.Range(0, quantity);
            selection = targets[index];
            if (selection == null)
            {
                RemoveByIndex(index);
            }
            if (ReferenceEquals(selection, excluding))
            {
                selection = null; // Prevent objects from selecting themselves
            }
        }
        return selection;
    }

    public PlayerCar GetPlayer()
    {
        for (int index = 0; index < quantity; index++)
        {
            GameObject currentObject = targets[index];

            if (currentObject != null && currentObject.CompareTag("PlayerVehicle"))
            {
                return currentObject.GetComponent<PlayerCar>();
            }
        }

        return null;
    }

    public void ClearAll()
    {
        for (int index = 0; index < quantity; index++)
        {
            targets[index] = null;
        }

        quantity = 0;
        enemiesAreDestroyed = false;
        bossIsDestroyed = false;
    }

    public int returnQuantity() {
        return quantity;
    }
}

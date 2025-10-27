using System;
using System.Collections;
using UnityEngine;

public class ItemLifeSpan : MonoBehaviour
{
    [SerializeField] private float lifeSpan;
    [SerializeField] private bool isOnBomb;

    private Animator _animator;
    
    public static Action<Vector2Int> OnItemDestroyed;

    private void OnEnable()
    {
        Player.OnPreciousFoodEaten += Destroy;

        if (isOnBomb)
            _animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        Player.OnPreciousFoodEaten -= Destroy;
    }

    private void Destroy()
    {
        StartCoroutine(DestroyAfterTime());
    } 
        
    private IEnumerator DestroyAfterTime()
    {
        if (isOnBomb) _animator.enabled = true;
        yield return new WaitForSeconds(lifeSpan);
        OnItemDestroyed?.Invoke(Vector2Int.RoundToInt(this.transform.position));
        Destroy(gameObject);
    }
}

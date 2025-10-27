using System.Collections;
using UnityEngine;

public class SnakeBody : MonoBehaviour
{
    [SerializeField] private GameObject blowPrefab;
    [SerializeField] private Animator animator;

    private bool _isHead;

    private void OnEnable()
    {
        /*Player.OnHitAction*/
        Player.OnHitAction += OnHit;
    }
    private void OnDisable()
    {
        Player.OnHitAction -= OnHit;
    }

    public void SetHead()
    {
        animator.enabled = true;
        _isHead = true;
    }

    private void OnHit()
    {
        if (_isHead) {
            animator.enabled = true;
            animator.SetBool("Die", true);
            StartCoroutine(ResetSnakeState());
        }
    }

    IEnumerator ResetSnakeState()
    {
        yield return new WaitForSeconds(2f);
        animator.SetBool("Die", false);
    }
}

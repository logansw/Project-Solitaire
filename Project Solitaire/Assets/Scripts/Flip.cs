using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flip : MonoBehaviour
{
    [SerializeField] private float timeBetweenRotate;
    [SerializeField] private float rotateDuration;
    [SerializeField] private float orbitSpeed;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = timeBetweenRotate;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            StartCoroutine(RotateQuickly(rotateDuration));
            timer = timeBetweenRotate;
        }
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    private IEnumerator RotateQuickly(float rotateDuration)
    {
        float percentRotated = 0;
        float rotateTimer = 0;

        while (percentRotated < 1)
        {
            percentRotated = NormalizedLogisticFunction(rotateTimer / rotateDuration);
            rotateTimer += Time.deltaTime;
            gameObject.transform.eulerAngles = new Vector3(
                gameObject.transform.eulerAngles.x,
                360 * percentRotated,
                gameObject.transform.eulerAngles.z
            );
            yield return 0;
        }
        gameObject.transform.eulerAngles = new Vector3(
            gameObject.transform.eulerAngles.x,
            0,
            gameObject.transform.eulerAngles.z
        );
    }

    private float NormalizedLogisticFunction(float x)
    {
        return (1.01f / (1 + Mathf.Pow(2.71828f, -15 * (x - 0.5f))));
    }
}

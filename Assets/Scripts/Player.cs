using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    [Header("Player stats")]
    [SerializeField] private float speed = 50;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float throwForce = 5;

    [Header("Product Management")]
    public Product closestProduct;
    public Product holdProduct;
    public Transform holdParent;

    [Header("Dash")]
    public float dashRange;
    public float dashDuration = .25f;
    public float dashCD = 1;
    private float baseDashCD;
    public bool dashing;

    private Vector2 movement;
    private Rigidbody rb;
    private Transform gfx;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gfx = transform.GetChild(0);

        baseDashCD = dashCD;
    }

    void FixedUpdate()
    {
        if (dashCD > 0) dashCD -= Time.deltaTime;

        if (dashing) return;

        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.y) * speed * Time.fixedDeltaTime;
        gfx.forward = Vector3.Lerp(gfx.forward, new Vector3(movement.x, 0, movement.y), rotateSpeed);
    }

    #region Input

    public void OnMove(CallbackContext context) => movement = context.ReadValue<Vector2>();

    public void OnDash(CallbackContext context) => context.action.started += _ => Dash();

    public void OnThrow(CallbackContext context) => context.action.started += _ => ThrowProduct();

    public void OnGrab(CallbackContext context) => context.action.started += _ => Grab();

    #endregion

    private void Dash()
    {
        // If input is neutral return
        if (movement.x == 0 && movement.y == 0 || dashing || dashCD > 0) return;
        dashing = true;
        dashCD = baseDashCD;
        StartCoroutine(DashCO());
    }

    private IEnumerator DashCO()
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        float startTime = Time.time;
        rb.velocity = Vector3.zero;

        while (Time.time < startTime + dashDuration)
        {
            rb.velocity += new Vector3(movement.x, 0, movement.y) * dashRange;
            yield return waitForFixedUpdate;
        }
        dashing = false;
    }

    private void ThrowProduct()
    {
        if (holdProduct != null)
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.rb.AddForce(gfx.forward * throwForce, ForceMode.Impulse);
            holdProduct = null;
        }
    }

    private void Grab()
    {
        if (holdProduct != null)
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct = null;
        }
        else if (closestProduct != null)
        {
            if (!closestProduct.gameObject.activeInHierarchy) // If product is from shelf - instantiate 
            {
                Product product = Instantiate(closestProduct.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Product>();
                holdProduct = product;
            }
            else holdProduct = closestProduct.GetComponent<Product>();

            holdProduct.transform.SetParent(holdParent);
            holdProduct.transform.localPosition = Vector3.zero;
            holdProduct.transform.localEulerAngles = Vector3.zero;
            holdProduct.rb.isKinematic = true;

        }
    }
}

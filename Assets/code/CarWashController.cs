using System.Collections;
using UnityEngine;

public class CarWashController : MonoBehaviour
{
    [Header("--- 1. TỌA ĐỘ & KHUNG GANTRY ---")]
    public Transform gantryTransform;
    public Transform pointStart;            // Đầu xe
    public Transform pointEnd;              // Đuôi xe
    public float moveDuration = 8.0f;       // Thời gian di chuyển 1 chiều

    [Header("--- 2. CỤM HIỆU ỨNG (SPRAY, FOAM, DRYER) ---")]
    public GameObject sprayGroup;           // Vòi xịt nước (Dùng cho Bước 1, 3, 4)
    public GameObject foamGroup;            // Vòi phun bọt tuyết (Bước 2)
    public GameObject dryerGroup;           // Hệ thống quạt sấy (Bước 5)

    [Header("--- 3. BƯỚC 3: RỬA BÁNH TRƯỚC & BÁNH SAU ---")]
    public Transform wheelBrushLeft;        
    public Transform wheelBrushRight;       
    
    [Range(0.0f, 1.0f)]
    public float frontWheelRatio = 0.3f;    
    [Range(0.0f, 1.0f)]
    public float rearWheelRatio = 0.7f;     
    
    public float wheelWashDuration = 4.0f;  
    public float wheelBrushRotateSpeed = 600f;

    [Header("--- 4. BƯỚC 4: 3 CHỔI CỌ VỎ XE ---")]
    public Transform brushTop;              
    public float brushTopHighY = 2.2f;       
    public float brushTopLowY = 1.3f;        
    public float brushMoveDuration = 2.0f;
    
    [Space(5)]
    public Transform brushVerticalLeft;     
    public Transform brushVerticalRight;    
    public float bodyBrushRotateSpeed = 400f;

    [Header("--- 5. BẢNG ĐÈN LED TƯỜNG (5 BƯỚC) ---")]
    public Renderer[] stageLeds;            
    public Material ledOffMaterial;        
    public Material ledOnMaterial;         

    private bool isSpinningWheelBrushes = false;
    private bool isSpinningBodyBrushes = false;
    private bool isWashing = false;

    private void Start()
    {
        // Tắt triệt để và dọn sạch hiệu ứng ngay khi mở Scene
        StopAndHideEffect(sprayGroup);
        StopAndHideEffect(foamGroup);
        StopAndHideEffect(dryerGroup);

        SetAllLEDsOff();

        if (gantryTransform != null && pointStart != null)
        {
            Vector3 startPos = gantryTransform.position;
            startPos.z = pointStart.position.z;
            gantryTransform.position = startPos;
        }

        if (brushTop != null)
        {
            Vector3 topPos = brushTop.localPosition;
            topPos.y = brushTopHighY;
            brushTop.localPosition = topPos;
        }
    }

    private void Update()
    {
        // Xoay chổi mâm bánh xe ở Bước 3
        if (isSpinningWheelBrushes)
        {
            float rotStep = wheelBrushRotateSpeed * Time.deltaTime;
            if (wheelBrushLeft) wheelBrushLeft.Rotate(Vector3.up, rotStep, Space.Self);
            if (wheelBrushRight) wheelBrushRight.Rotate(Vector3.up, -rotStep, Space.Self);
        }

        // Xoay 3 chổi cọ vỏ xe ở Bước 4
        if (isSpinningBodyBrushes)
        {
            float rotStep = bodyBrushRotateSpeed * Time.deltaTime;
            if (brushTop) brushTop.Rotate(Vector3.up, rotStep, Space.Self);
            if (brushVerticalLeft) brushVerticalLeft.Rotate(Vector3.up, rotStep, Space.Self);
            if (brushVerticalRight) brushVerticalRight.Rotate(Vector3.up, -rotStep, Space.Self);
        }
    }

    public void StartWashProcess()
    {
        StartCarWashProcess();
    }

    public void StartCarWashProcess()
    {
        if (!isWashing)
        {
            StartCoroutine(CarWashSequenceRoutine());
        }
    }

    private IEnumerator CarWashSequenceRoutine()
    {
        isWashing = true;
        yield return new WaitForSeconds(1.0f);

        // =========================================================================
        // BƯỚC 1: XẢ NƯỚC RỬA SƠ BỘ (Start -> End)
        // =========================================================================
        SetStageLED(0);
        PlayEffect(sprayGroup);
        yield return StartCoroutine(MoveGantry(pointStart.position.z, pointEnd.position.z, moveDuration));
        StopAndHideEffect(sprayGroup);
        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // BƯỚC 2: PHUN BỌT TUYẾT PHỦ THÂN XE (End -> Start)
        // =========================================================================
        SetStageLED(1);
        PlayEffect(foamGroup);
        yield return StartCoroutine(MoveGantry(pointEnd.position.z, pointStart.position.z, moveDuration));
        StopAndHideEffect(foamGroup);
        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // BƯỚC 3: CỌ MÂM BÁNH TRƯỚC VÀ BÁNH SAU + XẢ NƯỚC
        // =========================================================================
        SetStageLED(2);
        
        float frontWheelZ = Mathf.Lerp(pointStart.position.z, pointEnd.position.z, frontWheelRatio);
        float rearWheelZ  = Mathf.Lerp(pointStart.position.z, pointEnd.position.z, rearWheelRatio);

        // Bật vòi xịt nước rửa mâm bánh
        PlayEffect(sprayGroup);

        // 3A. Bánh trước (Gantry di chuyển tới bánh trước -> Xoay chổi cọ mâm)
        yield return StartCoroutine(MoveGantry(pointStart.position.z, frontWheelZ, moveDuration * 0.3f));
        isSpinningWheelBrushes = true;
        yield return StartCoroutine(ScrubWheelAtPosition(frontWheelZ, wheelWashDuration));

        // 3B. Bánh sau (Gantry di chuyển tiếp tới bánh sau -> Xoay chổi cọ mâm)
        yield return StartCoroutine(MoveGantry(frontWheelZ, rearWheelZ, moveDuration * 0.4f));
        yield return StartCoroutine(ScrubWheelAtPosition(rearWheelZ, wheelWashDuration));

        isSpinningWheelBrushes = false; // Tắt chổi mâm
        StopAndHideEffect(sprayGroup);  // Tắt vòi xịt nước

        // Lùi Gantry về Start
        yield return StartCoroutine(MoveGantry(gantryTransform.position.z, pointStart.position.z, moveDuration * 0.5f));
        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // BƯỚC 4: 3 CHỔI XOAY CỌ RỬA VỎ XE + XẢ NƯỚC RỬA (Start -> End)
        // =========================================================================
        SetStageLED(3);

        // Hạ chổi top
        yield return StartCoroutine(MoveBrushTopY(brushTopHighY, brushTopLowY, brushMoveDuration));

        isSpinningBodyBrushes = true; // Xoay 3 chổi cọ
        PlayEffect(sprayGroup);       // Bật vòi xịt nước trơn cọ vỏ xe

        yield return StartCoroutine(MoveGantry(pointStart.position.z, pointEnd.position.z, moveDuration));

        isSpinningBodyBrushes = false; // Tắt 3 chổi
        StopAndHideEffect(sprayGroup);  // Tắt vòi xịt nước

        // Nâng chổi top nghỉ
        yield return StartCoroutine(MoveBrushTopY(brushTopLowY, brushTopHighY, brushMoveDuration));
        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // BƯỚC 5: THỔI KHÔ BẰNG QUẠT SẤY (End -> Start)
        // =========================================================================
        SetStageLED(4);
        PlayEffect(dryerGroup);

        yield return StartCoroutine(MoveGantry(pointEnd.position.z, pointStart.position.z, moveDuration));

        StopAndHideEffect(dryerGroup);
        yield return new WaitForSeconds(0.5f);

        // =========================================================================
        // HOÀN THÀNH QUY TRÌNH
        // =========================================================================
        Debug.Log(">>> QUY TRÌNH RỬA XE TỰ ĐỘNG 5 BƯỚC HOÀN THÀNH! <<<");
        isWashing = false;
    }

    private IEnumerator ScrubWheelAtPosition(float centerZ, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float offset = Mathf.Sin(timer * 8f) * 0.12f;
            Vector3 pos = gantryTransform.position;
            pos.z = centerZ + offset;
            gantryTransform.position = pos;
            yield return null;
        }
    }

    private IEnumerator MoveGantry(float startZ, float targetZ, float duration)
    {
        if (gantryTransform == null) yield break;

        float elapsed = 0f;
        Vector3 pos = gantryTransform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newZ = Mathf.Lerp(startZ, targetZ, elapsed / duration);
            pos.z = newZ;
            gantryTransform.position = pos;
            yield return null;
        }

        pos.z = targetZ;
        gantryTransform.position = pos;
    }

    private IEnumerator MoveBrushTopY(float startY, float targetY, float duration)
    {
        if (brushTop == null) yield break;

        float elapsed = 0f;
        Vector3 localPos = brushTop.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            localPos.y = Mathf.Lerp(startY, targetY, elapsed / duration);
            brushTop.localPosition = localPos;
            yield return null;
        }

        localPos.y = targetY;
        brushTop.localPosition = localPos;
    }

    // Bật Particle System
    private void PlayEffect(GameObject objGroup)
    {
        if (objGroup != null)
        {
            objGroup.SetActive(true);
            ParticleSystem ps = objGroup.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    // Tắt và dọn sạch các hạt Particle System
    private void StopAndHideEffect(GameObject objGroup)
    {
        if (objGroup != null)
        {
            ParticleSystem ps = objGroup.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            objGroup.SetActive(false);
        }
    }

    private void SetStageLED(int activeIndex)
    {
        if (stageLeds == null || stageLeds.Length == 0) return;

        for (int i = 0; i < stageLeds.Length; i++)
        {
            if (stageLeds[i] != null)
            {
                stageLeds[i].material = (i == activeIndex) ? ledOnMaterial : ledOffMaterial;
            }
        }
    }

    private void SetAllLEDsOff()
    {
        if (stageLeds == null) return;
        foreach (var led in stageLeds)
        {
            if (led != null) led.material = ledOffMaterial;
        }
    }
}
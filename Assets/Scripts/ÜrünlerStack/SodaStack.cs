    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;

    public class SodaStack : MonoBehaviour
    {
        [Header("Soda Ayarları")]
        public GameObject sodaPrefab;
        public Transform stackRoot;
        public float cubeHeight = 0.005f;
        public float tweenDuration = 0.3f;
        public Ease tweenEase = Ease.OutCubic;
        public int maxStack = 10;
        public float spawnDelay = 0.4f;

        [Header("Bırakma Ayarları")]
        public Transform sodaDropTarget;  // Sodaların bırakılacağı yer
        public float dropSpacing = 0.002f; // Bırakılan sodaların arası mesafe

        private List<Transform> sodaStack = new List<Transform>();
        private List<Transform> droppedSodas = new List<Transform>(); // Bırakılanlar listesi
        private bool canCollect = false;
        private bool isInDropArea = false;
        private Coroutine collectRoutine;
        private Coroutine dropRoutine;

        private void Update()
        {
            UpdateStackPositions(); // sürekli hizalama
        }

        private void UpdateStackPositions()
        {
            for (int i = 0; i < sodaStack.Count; i++)
            {
                Transform soda = sodaStack[i];
                float yOffset = 0.0005f;
                Vector3 targetPos = stackRoot.position + Vector3.up * (cubeHeight * i + yOffset);

                soda.position = targetPos;
                soda.rotation = Quaternion.identity;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("SodaNoktasi"))
            {
                if (!canCollect)
                {
                    canCollect = true;
                    collectRoutine = StartCoroutine(CollectSodaRoutine());
                }
            }

            if (other.CompareTag("StackSilmeNoktasi0"))
            {
                isInDropArea = true;
                if (dropRoutine == null)
                    dropRoutine = StartCoroutine(DropSodasRoutine());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("SodaNoktasi"))
            {
                canCollect = false;
                if (collectRoutine != null)
                    StopCoroutine(collectRoutine);
            }

            if (other.CompareTag("StackSilmeNoktasi0"))
            {
                isInDropArea = false;
                if (dropRoutine != null)
                {
                    StopCoroutine(dropRoutine);
                    dropRoutine = null;
                }
            }
        }

        private IEnumerator CollectSodaRoutine()
        {
            while (canCollect && sodaStack.Count < maxStack)
            {
                AddSoda();
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void AddSoda()
        {
            Vector3 spawnPos = stackRoot.position + Vector3.up * (cubeHeight * sodaStack.Count);
            GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity);

            newSoda.transform.localScale = Vector3.zero;
            newSoda.transform.SetParent(stackRoot);

            // Daha küçük boyut (0.0005f) ve OutCubic ease
            newSoda.transform.DOScale(Vector3.one * 0.0012f, tweenDuration).SetEase(tweenEase);

            sodaStack.Add(newSoda.transform);
        }

        private IEnumerator DropSodasRoutine()
        {
            while (isInDropArea && sodaStack.Count > 0)
            {
                // Son sodayı al
                Transform soda = sodaStack[sodaStack.Count - 1];
                sodaStack.RemoveAt(sodaStack.Count - 1);

                soda.SetParent(null); // root’tan ayır

                // Hedef pozisyon (üst üste dizilecek)
                int dropIndex = droppedSodas.Count;
                Vector3 targetPos = sodaDropTarget.position + Vector3.up * (cubeHeight * dropIndex);

                // Animasyonla bırak (üst üste stacklensin)
                soda.DOJump(targetPos, 0.002f, 1, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { soda.rotation = Quaternion.identity; });

                droppedSodas.Add(soda);

                yield return new WaitForSeconds(0.1f); // biraz bekle, tek tek bıraksın
            }
        }

    }

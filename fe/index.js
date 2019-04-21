void async function() {
  const payload = new TextEncoder().encode('test');
  // TODO: Derive a key from a passphrase https://stackoverflow.com/q/45636545/2715716
  const key = await window.crypto.subtle.generateKey({ name: 'AES-CBC', length: 256 }, true, ['encrypt', 'decrypt']);
  // Note that for AES-CBC the IV length is the same as the block size: 128 bits - 16 bytes
  const iv = Uint8Array.from([ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 ]);
  const data = new Uint8Array(await window.crypto.subtle.encrypt({ name: 'AES-CBC', iv }, key, payload));
  const response = await fetch('http://localhost:8000/api', { method: 'POST', body: new Blob([payload]) });
}()

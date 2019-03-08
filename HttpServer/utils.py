from binascii import unhexlify
from hashlib import pbkdf2_hmac
from os import urandom
from time import ctime

from bitcoin import (N, add_privkeys, add_pubkeys, decode_privkey,
                     decode_pubkey, encode_privkey, encode_pubkey,
                     privkey_to_pubkey, pubkey_to_address)

__all__ = [
    'ctime',
    'isValidPrivKey',
    'decode_privkey', 'decode_pubkey',
    'encode_privkey', 'encode_pubkey',
    'pubkey_to_address', 'privkey_to_pubkey',
    'add_privkeys', 'add_pubkeys',
    'urandom',
    'unhexlify', 'pbkdf2_hmac'
]


def isValidPrivKey(key):
    return (key > 0) and (key < N)


def read_key_bytes(filename):
    with open(filename, 'rb') as f:
        key = f.read()
    return key


def read_master_cipher_private_key():
    return read_key_bytes('cipher.priv')


def read_master_cipher_public_key():
    return read_key_bytes('cipher.pub')


def read_master_bitcoin_public_key():
    return read_key_bytes('bitcoin.pub')

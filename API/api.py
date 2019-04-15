import os
import sys
import random
import math
import re
import time
import numpy as np
import tensorflow as tf
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as patches
import json
import cv2
from flask import Flask, request, jsonify
from PIL import Image
from keras.backend import clear_session
import datetime as datetime

#----------------------------
# RCNN IMPORTS
#----------------------------

from mrcnn import utils
from mrcnn import visualize
from mrcnn.visualize import display_images
import mrcnn.model as modellib
from mrcnn.model import log
import cups as cup

#----------------------------
# RCNN IMPORTS
#----------------------------
import keypoint.main as kp

app = Flask(__name__)

sys_config=json.load(open("config.json", 'r'))

%matplotlib inline

import base64
import skimage.io

# This is requred to change base64 into a numpy ndarray

def decode(base64_string):
    if isinstance(base64_string, bytes):
        base64_string = base64_string.decode("utf-8")
    imgdata = base64.b64decode(base64_string)
    img = skimage.io.imread(imgdata, plugin='imageio')
    return img

def get_ax(rows=1, cols=1, size=16):
    #This fn essentially allows a base size for graphs below
    #Common thing i've seen in notebooks with matplotlib
    _, ax = plt.subplots(rows, cols, figsize=(size*cols, size*rows))
    return ax

config = cup.CupConfig() #Both configs are the same, we just use the one in the cup file for ease

class Inference_Config(config.__class__):
    # Make sure we only run detection 1 at a time
    # This value may be increased when moved to cloud
    GPU_COUNT = 1
    IMAGES_PER_GPU = 1
    DETECTION_MIN_CONFIDENCE=0.995
config = Inference_Config()

def create_cup_model():
    with tf.device(sys_config["device"]):
        clear_session()
        global cup_model
        cup_model = modellib.MaskRCNN(mode=sys_config["mode"], model_dir=sys_config["cup"]["model directory"],
                                  config=config)
    try:
        print("Loading cup weights ", sys_config["cup"]["weights path"])
        cup_model.load_weights(sys_config["cup"]["weights path"], by_name=True)
    except:
        print("Cup weights file unable to be loaded".format(error))
        
def create_bowl_model():
    with tf.device(sys_config["device"]):
        clear_session()
        global bowl_model
        bowl_model = modellib.MaskRCNN(mode=sys_config["mode"], model_dir=sys_config["bowl"]["model directory"],
                                  config=config)
    try:
        print("Loading bowl weights ", sys_config["bowl"]["weights path"])
        bowl_model.load_weights(sys_config["bowl"]["weights path"], by_name=True)
    except:
        print("Bowl weights file unable to be loaded".format(error))
        
#rois: [N, (y1, x1, y2, x2)] detection bounding boxes
def final_RCNN_ret(image, roi, obj):
    img=image[roi[0]:roi[2], roi[1]:roi[3]]
    if obj == "cup":
        cropped_imgs.append(img)
    centre = find_box_center(roi, image)
    return {"centre": centre, "object_type":obj}

def find_box_center(roi, image):
    bl=(roi[1], roi[2])
    br=(roi[3], roi[2])
    tl=(roi[1], roi[0])
    tr=(roi[3], roi[0])
    true_y = (roi[0]+roi[2])/2
    true_x = (roi[1]+roi[3])/2
    print("0: ", roi[0]," | 1: ", roi[1]," | 2: ", roi[2]," | 3: ", roi[3],)
    print("br: ", br, " | bl: ", bl, " | tl: ", tl, " | tr: ", tr, )
    rel_roi = []
    for i in roi:
        #The shape of the image is normalised to square so we don't need to
        #worry about different x and y shapes
        rel_roi.append((i/image.shape[0])*100)
    rel_y = np.round((rel_roi[0]+rel_roi[2])/2, 2)
    rel_x = np.round((rel_roi[1]+rel_roi[3])/2, 2)

    return ([true_y, true_x, rel_y, rel_x])

def detect_boxes_and_crop(objects):
    global imgs
    imgs=[]
    for obj in request.json['objects']:
        if obj == "Cup":
            create_cup_model()
            detect_cup_in_img()
        if obj == "Bowl":
            create_bowl_model()
            detect_bowl_in_img()
    #return json.dumps({"crops":imgs})
    return imgs

def detect_cup_in_img():
    prediction = cup_model.detect([image])[0]
    for roi in prediction['rois']:
        plt.axis('off')
        imgs.append(final_RCNN_ret(image, roi, "cup"))
    
def detect_bowl_in_img():
    prediction = bowl_model.detect([image])[0]
    for roi in prediction['rois']:
        plt.axis('off')
        box_ret = final_RCNN_ret(image, roi, "bowl")
        imgs.append(box_ret)   
        
def default_hparams():
    hparams = tf.contrib.training.HParams(
      num_filters=64,  # Number of filters.
      num_kp=10,  # Numer of keypoints.

      loss_pose=0.2,  # Pose Loss.
      loss_con=1.0,  # Multiview consistency Loss.
      loss_sep=1.0,  # Seperation Loss.
      loss_sill=1.0,  # Sillhouette Loss.
      loss_lr=1.0,  # Orientation Loss.
      loss_variance=0.5,  # Variance Loss (part of Sillhouette loss).

      sep_delta=0.05,  # Seperation threshold.
      noise=0.1,  # Noise added during estimating rotation.

      learning_rate=1.0e-3,
      lr_anneal_start=5000,  # When to anneal in the orientation prediction.
      lr_anneal_end=7000,  # When to use the prediction completely.
    )
    return hparams

def avg(array):
    count = 0
    for i in array:
        count+=i
    return count / array.size

tf.reset_default_graph()
hp = default_hparams()

def keypoint_predict(input_img, hparams):
    #Reset the tensorflow graph each call
    tf.reset_default_graph()
    

    #Set the size of the image we need (128x128)
    img = tf.placeholder(tf.float32, shape=(1, 128, 128, 4))

    with tf.variable_scope("KeypointNetwork"):
        #Generate the network based on the placeholder image size
        ret = kp.keypoint_network(img, hparams.num_filters, hparams.num_kp, False)

    uv = tf.reshape(ret[0], [-1, hparams.num_kp, 2])
    z = tf.reshape(ret[1], [-1, hparams.num_kp, 1])
    uvz = tf.concat([uv, z], axis=2)
    orient =ret[2]

    sess = tf.Session()
    saver = tf.train.Saver()
    ckpt = tf.train.get_checkpoint_state(sys_config["keypoint"]["checkpoint"])

    print("loading keypoint model...")
    saver.restore(sess, ckpt.model_checkpoint_path)

    orig = input_img.astype(float) / 255
    if orig.shape[2] == 3:
        orig = np.concatenate((orig, np.ones_like(orig[:, :, :1])), axis=2)

    uv_ret = sess.run(uvz, feed_dict={img: np.expand_dims(orig, 0)})
    z_ret = sess.run(z, feed_dict={img: np.expand_dims(orig, 0)})
    orient_ret = sess.run(orient, feed_dict={img: np.expand_dims(orig, 0)})

    return {"Average Depth":str(avg(z_ret[0])[0]), "Orientation":orient_ret.tolist(), "Points":uv_ret[0].tolist()}

def keypoint_on_crops():
    kps=[]
    print(len(cropped_imgs))
    for img in cropped_imgs:
        img = cv2.resize(img, dsize=(128, 128), interpolation=cv2.INTER_CUBIC)
        kps.append(keypoint_predict(img, hp))
    return kps

@app.route(sys_config['api']['base uri'], methods=['PUT'])
def detection():
    if not request.json:
        abort(400)
    global cropped_imgs
    cropped_imgs = []
    global image
    image = decode(request.json['image'])
    image = cv2.resize(image, dsize=(500, 500), interpolation=cv2.INTER_CUBIC)
    rcnn = detect_boxes_and_crop(request.json['objects'])
    keypoint = keypoint_on_crops()
    return json.dumps({"crops":rcnn, "keypoints":keypoint}), 200

if __name__ == '__main__':
    app.run(debug=False)
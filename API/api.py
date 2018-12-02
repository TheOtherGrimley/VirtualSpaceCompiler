# FILE NEEDS MODULARISED

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
from flask import Flask, request, jsonify
from PIL import Image
from keras.backend import clear_session
import datetime as datetime
import base64
import skimage.io

#----------------------------
# RCNN IMPORTS
#----------------------------

from mrcnn import utils
from mrcnn import visualize
from mrcnn.visualize import display_images
import mrcnn.model as modellib
from mrcnn.model import log
import cups as cup

app = Flask(__name__)

sys_config=json.load(open("config.json", 'r'))

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

config = cup.CupConfig()

class InferenceConfig(config.__class__):
    # Make sure we only run detection 1 at a time
    # This value may be increased when moved to cloud
    GPU_COUNT = 1
    IMAGES_PER_GPU = 1
    DETECTION_MIN_CONFIDENCE=0.96
config = InferenceConfig()


def create_model():
    with tf.device(sys_config["device"]):
        clear_session()
        global model
        model = modellib.MaskRCNN(mode=sys_config["mode"], model_dir=sys_config["model directory"],
                                  config=config)
    try:
        print("Loading weights ", sys_config["weights path"])
        model.load_weights(sys_config["weights path"], by_name=True)
    except:
        print("Weights file unable to be loaded".format(error))
        
#rois: [N, (y1, x1, y2, x2)] detection bounding boxes
def final_ret(image, roi):
    img=image[roi[0]:roi[2], roi[1]:roi[3]]
    centre = find_box_center(roi, image)
    plt.imshow(img)
    filename="detect_{:%Y%m%dT%H%M%S}.png".format(datetime.datetime.now())
    plt.savefig(filename)
    return json.dumps({"image":img.tolist(), "centre": centre, "filename":filename})

def find_box_center(roi, image):
    true_y = (roi[0]+roi[2])/2
    true_x = (roi[1]+roi[3])/2
    rel_roi = []
    for i in roi:
        #The shape of the image is normalised to square so we don't need to
        #worry about different x and y shapes
        rel_roi.append((i/image.shape[0])*100)
    rel_y = np.round((rel_roi[0]+rel_roi[2])/2, 2)
    rel_x = np.round((rel_roi[1]+rel_roi[3])/2, 2)

    return ([true_y, true_x, rel_y, rel_x])

def detect_boxes_and_crop(image):
    create_model()
    prediction = model.detect([image])[0]
    imgs=[]
    for roi in prediction['rois']:
        plt.axis('off')
        box_ret = final_ret(image, roi)
        imgs.append(box_ret)
    return imgs

@app.route(sys_config['api']['base uri'], methods=['POST'])
def detect_cup_in_img():
    if not request.json:
        abort(400)
    image = decode(request.json['image'])
    cropped_images=detect_boxes_and_crop(image)
    return jsonify(cropped_images), 200

if __name__ == '__main__':
    app.run(debug=False)
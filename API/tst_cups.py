### This class is based off the coco.py class that comes with mask R-CNN. On building ###
### On building, i went through and commented with comments that make sense to me     ###
import os
import sys
import json
import datetime
import numpy as np
import skimage.draw

from mrcnn.config import Config
from mrcnn import model as modellib, utils

ROOT_DIR = os.path.abspath("C:\\Users\\AdamG\\OneDrive\\Documents\\Projects\\Uni\\FYP\\API\\")

# Path to trained weights file
COCO_WEIGHTS_PATH = os.path.join(ROOT_DIR, "mask_rcnn_coco.h5")

DEFAULT_LOGS_DIR = os.path.join(ROOT_DIR, "logs")

class CupConfig(Config):
    NAME="cup"
    IMAGES_PER_GPU = 1
    NUM_CLASSES = 1 + 1
    STEPS_PER_EPOCH = 100
    DETECTION_MIN_CONFIDENCE = 0.85
    
class InferenceConfig(CupConfig):
    GPU_COUNT=1
    IMAGES_PER_GPU=1

class CupDataset(utils.Dataset):
    def load_cup(self,dataset_dir,subset):
        #dataset_dir => dataset directory location
        #subset => which subset you are using, training or validation
        
        #Add single class to the dataset
        self.add_class("cup",1,"cup")
        
        #Make sure that the subset is valid
        assert subset in ["train","val"]
        dataset_dir = os.path.join(dataset_dir, subset)
        
        #When using VGG.via annotator to annotate images, the annotations are
        #exprted as "via_region_data.json". this file should be in with your dataset
        annotations = json.load(open(os.path.join(dataset_dir, "via_region_data.json")))
        annotations = list(annotations.values())
                 
        #list comprehension and JSON scraping, ah what a world...
        #remove any unannotated images
        annotations = [a for a in annotations if a['regions']]
        
        for annotation in annotations:
            #Grab the attributes in the VGG.via json for the image
            polygons = [region['shape_attributes'] for region in annotation['regions']]
            #TODO: from the mask rcnn system, we need to read the image to convert
            #the polygon VGG.via output into masks. There may be no need for this
            
            #Initial plans are to pass a base64 into the api. This may need changed
            image_path = os.path.join(dataset_dir,annotation['filename'])
            image = skimage.io.imread(image_path)
            height, width = image.shape[:2]
            
            self.add_image(
                "cup",
                image_id=annotation['filename'],
                path=image_path,
                width = width, 
                height = height,
                polygons = polygons)
            
    def load_mask(self,image_id):
        image_info = self.image_info[image_id]
        #Make sure it's a cup we're looking at
        if image_info['source'] != "cup":
            return super(self.__class__, self).load_mask(image_id)
        
        #convert polygons to a bitmap mask
        #zeros([height,width,instance_count], data-type)
        info = self.image_info[image_id]
        mask = np.zeros([info['height'],info['width'], len(info['polygons'])],
                        dtype = np.uint8)
        #Set pixels in the polygon to 1
        for i, p in enumerate(info['polygons']):
            rr, cc = skimage.draw.polygon(p['all_points_y'], p['all_points_x'])
            mask[rr, cc, i] = 1
        
        return mask.astype(np.bool), np.ones([mask.shape[-1]], dtype=np.int32)
    
    def image_reference(self, image_id):
        info = self.image_info[image_id]
        if info['source'] == 'cup':
            return info['path']
        else:
            super(self.__class__, self).image_reference(image_id)
            
def train(model):
    # Training dataset.
    dataset_train = CupDataset()
    dataset_train.load_cup(args.dataset, "train")
    dataset_train.prepare()

    # Validation dataset
    dataset_val = CupDataset()
    dataset_val.load_cup(args.dataset, "val")
    dataset_val.prepare()

    # *** This training schedule is an example. Update to your needs ***
    # Since we're using a very small dataset, and starting from
    # COCO trained weights, we don't need to train too long. Also,
    # no need to train all layers, just the heads should do it.
    print("Training network heads")
    model.train(dataset_train, dataset_val,
                learning_rate=config.LEARNING_RATE,
                epochs=30,
                layers='heads')
            
## Training

if __name__ == '__main__':
    import argparse #This is for command line argument reading
    
    parser=argparse.ArgumentParser(
        description='Train R-CNN for cup detection [prototype]')
    parser.add_argument('command', metavar='<command>', help='train or splash')
    parser.add_argument('--dataset',required=False, metavar='/path/to/dataset/', help='directory of required dataset')
    parser.add_argument('--weights',required=True, metavar='/path/to/weights.h5', help='path to required weights h5 file')
    parser.add_argument('--logs',required=False, metavar='/path/to/logs/', help='directory to store logs and checkpoints')
    parser.add_argument('--image',required=False, metavar='/path/to/image or url', help='Image to apply the color splash effect on')
    parser.add_argument('--video',required=False, metavar='/path/to/video or url', help='Video to apply the color splash effect on')
    
    args=parser.parse_args()
    
    if args.command == 'train':
        config=CupConfig() #This overrides some values from the default config.py for M-RCNN
    else:
        config=InferenceConfig()
    
    config.display()
    
    # Create model
    if args.command == 'train':
        model=modellib.MaskRCNN(mode='training', config=config, model_dir=args.logs)
        
    else:
        model=modellib.MaskRCNN(mode='inference', config=config, model_dir=args.logs)
        
    # Select weights
    if args.weights.lower() == 'coco':
        weights_path=COCO_WEIGHTS_PATH
        #DL coco weights if it doesn't exist
        if not os.path.exists(weights_path):
            utils.download_trained_weights(weights_path)
    elif args.weights.lower() == 'last':
        weights_path=model.find_last()
    elif args.weights.lower() == 'imagenet':
        weights_path=model.get_imagenet_weights()
    else:
        weights_path=args.weights
        
    print('loading weights, hang on a second \n', weights_path)
    if args.weights.lower()=='coco':
        # Exclude the last layers because they require a matching
        # number of classes
        model.load_weights(weights_path, by_name=True, exclude=[
            "mrcnn_class_logits", "mrcnn_bbox_fc",
            "mrcnn_bbox", "mrcnn_mask"])
    else:
        model.load_weights(weights_path, by_name=True)
        
    if args.command.lower() == 'train':
        train(model)
    ### ADD IN COLOR SPLASH HERE I GUESS AT SOME POINT
    else:
        print('{} not recognised. Use "train"'.format(args.command))
            

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
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
            polygons = [region['shape_attributes'] for region in anno['regions']]
            #TODO: from the mask rcnn system, we need to read the image to convert
            #the polygon VGG.via output into masks. There may be no need for this
            
            #Initial plans are to pass a base64 into the api. This may need changed
            image_path = os.path.join(dataset_dir,annotation['filename'])
            image = skimage.io.imread(image_path)
            height, width = image.shape[:2]
            
            self.add_image(
                "cup",
                image_id=annotation['filename']
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
            
def train(model)
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
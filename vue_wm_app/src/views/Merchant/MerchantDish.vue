<script setup>
import { ref, onMounted, computed, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import { inject } from 'vue';
import axios from 'axios';
import { ElMessage } from 'element-plus';
import store from '@/store';

const router = useRouter();
const merchant = inject('merchant');
const dishes = ref([]);  // 用于存储菜品列表 
const searchQuery = ref(''); // 用于存储搜索框的内容 
const displayedDishes = ref([]); // 用于存储过滤后的菜品列表  
const isEditing = ref(false); // 用于判断当前是否在编辑模式  
const isCreating = ref(false); // 用于判断当前是否在创建模式 
const currentDish = ref(null); // 用于存储当前编辑的菜品  
const selectedImage = ref(null); // 用于存储用户选择的图片
const dishForm = ref(null); // 创建一个 ref 来引用 el-form
// 定义一个更新库存的方法  
const updateDishStock = async () => {

    try {
        const response = await axios.get(`http://39.105.5.211:5079/api/Merchant/dishSearch?merchantId=${merchant.value.MerchantId}`);
        dishes.value = response.data.data;  // 更新菜品列表  
        displayedDishes.value = dishes.value;  // 更新过滤后的菜品列表  
        ElMessage.success("菜品库存更新成功")
    } catch (error) {
        ElMessage.error("Error fetching dishes:", error);
    }
};
onMounted(async () => {
    merchant.value = store.state.merchant;
    await updateDishStock(); // 首次调用以获取菜品   
});

const editRules = computed(() => {
    const rules = {
        dishName: [
            { required: true, message: '请输入菜品名', trigger: 'blur' },
        ],
        dishPrice: [
            { required: true, message: '请输入菜品单价', trigger: 'blur' },
            {
                pattern: /^[0-9]*$/, // 正则表达式，确保输入是数字  
                message: '菜品单价只能是数字', // 错误提示消息  
                trigger: 'change',
            },
        ],
        dishCategory: [
            { required: true, message: '请输入类别描述', trigger: 'blur' },
        ],
        dishInventory: [
            { required: true, message: '请输入菜品库存', trigger: 'blur' },
            {
                pattern: /^[0-9]*$/, // 正则表达式，确保输入是数字  
                message: '菜品库存只能是数字', // 错误提示消息  
                trigger: 'change',
            },
        ],
    };
    return rules; // 返回动态生成的规则  
});
const validateField = (field) => {  //编辑规则的应用
    dishForm.value.validateField(field, (valid) => {
        if (!valid) {
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }
    });
};
// 搜索功能  
const searchDishes = () => {
    const query = searchQuery.value; // 获取搜索框内容  
    if (query) {
        displayedDishes.value = dishes.value.filter(dish =>
            dish.dishName.includes(query) || dish.dishCategory.includes(query)
        );
    } else {
        displayedDishes.value = dishes.value; // 如果没有搜索，则显示所有菜品  
    }
};
//删除功能  
const deleteDish = (dishId) => {
    axios.delete(`http://39.105.5.211:5079/api/Merchant/dishDelete`, {
        params: {
            merchantId: merchant.value.MerchantId,
            dishId: dishId
        }
    })
        .then(() => {
            const index = displayedDishes.value.findIndex(dish => dish.dishId === dishId);
            displayedDishes.value.splice(index, 1);
        })
        .catch(error => {
            if (error.response) {
                ElMessage.error('Error response:', error.response.data);
            } else {
                ElMessage.error('Error message:', error.message);
            }
        });
};
// 编辑功能  
const editDish = (dish) => {
    currentDish.value = { ...dish }; // 创建当前菜品的副本  
    isEditing.value = true; // 设置为编辑模式  
};
const cancelEdit = () => {
    isEditing.value = false; // 退出编辑模式  
    currentDish.value = null; // 清空当前菜品  
};
// 图片上传  
const handleImageChange = (event) => {
    selectedImage.value = event.target.files[0];
};

// 提交编辑  
const submitEdit = async () => {
    let imageUrl = currentDish.value.imageUrl; // 保持原来的图片 URL  
    if (selectedImage.value) {
        const formData = new FormData();
        formData.append('file', selectedImage.value);

        // 上传图片并获取 URL  
        try {
            const response = await axios.post('http://39.105.5.211:5079/api/Merchant/uploadImage', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            imageUrl = response.data.url;
        } catch (uploadError) {
            ElMessage.error('图片上传失败: ' + uploadError.message);
            return; // 终止提交  
        }
    }
    const isValid = await dishForm.value.validate(); // 使用 dishForm 引用进行验证  
    if (!isValid) return; // 如果不合法，提前退出  
    // 进行菜品更新  
    currentDish.value.imageUrl = imageUrl; // 更新图片 URL  
    currentDish.value.MerchantId = merchant.value.MerchantId; // 设置 MerchantId  
    console.log(currentDish.value);
    axios.put(`http://39.105.5.211:5079/api/Merchant/dishEdit`, currentDish.value) // 修改为 post 方法  
        .then(() => {
            const index = dishes.value.findIndex(dish => dish.dishId === currentDish.value.dishId);
            if (index !== -1) {
                dishes.value[index] = currentDish.value; // 更新原菜品列表  
                displayedDishes.value[index] = currentDish.value; // 更新显示的菜品列表  
            }
            ElMessage.success('修改成功！');
            isEditing.value = false; // 退出编辑模式  
            currentDish.value = null; // 清空当前菜品  
        })
        .catch(error => {
            // 根据错误码处理不同的错误  
            if (error.response && error.response.data) {
                const errorCode = error.response.data.errorCode;

                if (errorCode === 20000) {
                    ElMessage.error('没有更新数据');
                } else if (errorCode === 30000) {
                    ElMessage.error('连接失败' + error.response.data.msg);
                } else {
                    ElMessage.error('发生未知错误');
                }
            } else {
                ElMessage.error('网络错误，请重试');
            }
        });
};
// 提交创建新菜品  
const submitCreate = async () => {
    let imageUrl = 'https://img.zcool.cn/community/0138245e5218c4a80120a8950b14a0.png@1280w_1l_2o_100sh.png'; // 初始化图片 URL  

    if (selectedImage.value) {
        const formData = new FormData();
        formData.append('file', selectedImage.value);

        // 上传图片并获取 URL  
        try {
            const response = await axios.post('http://39.105.5.211:5079/api/Merchant/uploadImage', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            imageUrl = response.data.url; // 获取上传成功的图片 URL  
        } catch (uploadError) {
            ElMessage.error('图片上传失败: ' + uploadError.message);
            return; // 终止提交  
        }
    }
    const isValid = await dishForm.value.validate(); // 使用 dishForm 引用进行验证  
    if (!isValid) return; // 如果不合法，提前退出  

    // 创建新菜品  
    const newDish = {
        MerchantId: merchant.value.MerchantId,
        DishName: currentDish.value.dishName,
        DishId: 0,
        DishPrice: currentDish.value.dishPrice,
        DishCategory: currentDish.value.dishCategory,
        ImageUrl: imageUrl,
        DishInventory: currentDish.value.dishInventory
    };
    // 发送请求到创建接口  
    try {
        const response = await axios.post(`http://39.105.5.211:5079/api/Merchant/createDish`, newDish);
        const addDish = {
            merchantId: newDish.MerchantId,
            dishName: newDish.DishName,
            dishPrice: newDish.DishPrice,
            dishCategory: newDish.DishCategory,
            imageUrl: newDish.ImageUrl,
            dishInventory: newDish.DishInventory,
            dishId: response.data.data,
        };
        ElMessage.success('菜品创建成功！');
        isCreating.value = false; // 退出创建模式  
        dishes.value.push({ ...addDish }); // 将新创建的菜品添加到菜品列表  
        displayedDishes.value = [...dishes.value]; // 更新显示  
        currentDish.value = null; // 清空当前菜品  
        selectedImage.value = null; // 清空选择的图片  
    } catch (error) {
        // 根据错误码处理不同的错误  
        if (error.response && error.response.data) {
            const errorCode = error.response.data.errorCode;
            if (errorCode === 20000) {
                ElMessage.error('没有创建数据');
            } else if (errorCode === 30000) {
                ElMessage.error('图片上传失败: ' + error.response.data.msg);
            } else {
                ElMessage.error('发生未知错误');
            }
        } else {
            ElMessage.error('网络错误，请重试');
        }
    }
};
const startCreatingDish = () => {
    currentDish.value = {
        dishName: '',
        dishPrice: '',
        dishCategory: '',
        dishInventory: '',
        imageUrl: ''
    }; // 初始化为一个包含必要字段的对象  
    isCreating.value = true; // 进入创建模式  
    selectedImage.value = null; // 确保在创建新菜品时清空选择的图片  
};
// 取消创建操作  
const cancelCreate = () => {
    isCreating.value = false; // 退出创建模式  
    currentDish.value = null; // 清空当前菜品  
    selectedImage.value = null; // 清空选择的图片  
};  
</script>

<template>
    <slot name="sidebar"></slot>
    <div class="content">
        <header class="welcome-text">这里是菜单页面&nbsp;{{ merchant.merchantName }}</header>

        <div class="search-bar" v-if="!isEditing & !isCreating">
            <el-col :span="8">
                <el-input placeholder="请输入关键词..." v-model="searchQuery" clearable @clear="searchDishes"
                    @keydown.enter="searchDishes">
                    <template #append>
                        <el-button type="primary" @click="searchDishes"><el-icon>
                                <search />
                            </el-icon></el-button>
                    </template>
                </el-input>
            </el-col>
        </div>
        <!--
        <div class="search-container" v-if="!isEditing&!isCreating">  
            <input   
                type="text"   
                class="search-input"   
                placeholder="请输入关键词..."   
                v-model="searchQuery"  
                v-on:keyup.enter="searchDishes"  
            >  
            <button class="search-button" @click="searchDishes">搜索</button> 
            <button @click="startCreatingDish">新建菜单项</button> 
        </div>  
-->
        <div v-if="!isEditing & !isCreating" class="dishes-container">
            <div class="dishes-scroll">
                <ul class="dish-list">
                    <li v-for="dish in displayedDishes" :key="dish.dishId">
                        <img :src="dish.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
                        <span>&nbsp;&nbsp;{{ dish.dishName }}&nbsp;&nbsp;</span>
                        <span>价格: {{ dish.dishPrice }}&nbsp;&nbsp;</span>
                        <span>类别描述: {{ dish.dishCategory }}&nbsp;&nbsp;</span>
                        <span>库存：{{ dish.dishInventory }}&nbsp;&nbsp;</span>
                        <div class="right-group">
                            <button @click="editDish(dish)" class="button edit-btn">编辑</button>
                            <button @click="deleteDish(dish.dishId)" class="button delete-btn">删除</button>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
        <button @click="startCreatingDish" class="normal-button" v-if="!isEditing & !isCreating">新建菜单项</button>
        <!-- 编辑表单 -->
        <div v-if="isEditing" class="edit-container">
            <h2>编辑菜品</h2>
            <el-form :rules="editRules" ref="dishForm" :model="currentDish">
                <el-form-item label="图片上传" prop="selectedImage"><input type="file" @change="handleImageChange"
                        accept="image/*" /></el-form-item> <!-- 图片上传 -->
                <el-form-item label="菜品名" prop="dishName"><input v-model="currentDish.dishName" placeholder="菜品名"
                        @blur="validateField('dishName')" /></el-form-item>
                <el-form-item label="价格" prop="dishPrice"><input type="number" v-model="currentDish.dishPrice"
                        placeholder="价格" @blur="validateField('dishPrice')" /></el-form-item>
                <el-form-item label="类别描述" prop="dishCategory"><input v-model="currentDish.dishCategory"
                        placeholder="类别描述" @blur="validateField('dishCategory')" /> </el-form-item>
                <el-form-item label="库存" prop="dishInventory"><input type="number" v-model="currentDish.dishInventory"
                        placeholder="库存" @blur="validateField('dishInventory')" /> </el-form-item>
            </el-form>
            <button @click="submitEdit" class="submit-btn">提交</button>
            <button @click="cancelEdit" class="cancel-btn">取消</button>
        </div>
        <div v-if="isCreating" class="edit-container">
            <h2>创建菜品</h2>
            <el-form :rules="editRules" ref="dishForm" :model="currentDish">
                <el-form-item label="图片上传" prop="selectedImage"><input type="file" @change="handleImageChange"
                        accept="image/*" @blur="validateField('selectedImage')" /></el-form-item> <!-- 图片上传 -->
                <el-form-item label="菜品名" prop="dishName"><input v-model="currentDish.dishName" placeholder="菜品名"
                        @blur="validateField('dishName')" /></el-form-item>
                <el-form-item label="价格" prop="dishPrice"><input type="number" v-model="currentDish.dishPrice"
                        placeholder="价格" @blur="validateField('dishPrice')" /></el-form-item>
                <el-form-item label="类别描述" prop="dishCategory"><input v-model="currentDish.dishCategory"
                        placeholder="类别描述" @blur="validateField('dishCategory')" /> </el-form-item>
                <el-form-item label="库存" prop="dishInventory"><input type="number" v-model="currentDish.dishInventory"
                        placeholder="库存" @blur="validateField('dishInventory')" /> </el-form-item>
            </el-form>
            <button @click="submitCreate" class="submit-btn">提交</button>
            <button @click="cancelCreate" class="cancel-btn">取消</button>
        </div>
    </div>
</template>

<style scoped lang="scss">
.dish-list {
    list-style: none;
    padding: 0;
    margin-right: 0px;

    li {
        display: flex;
        align-items: center;
        padding: 10px;
        margin: 0 30px; // 修改左右外边距
        margin-bottom: 10px;
        border: 2px solid #ffee00;
        border-radius: 8px;
        background-color: #f9f9f9;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

        img {
            width: 50px;
            height: 50px;
            border-radius: 50%;
            margin-right: 15px;
        }

        span {
            margin-right: 15px;
            font-size: 14px;
            color: #333;
        }

        .right-group {
            margin-left: auto;
            /* 将右侧按钮推到右边 */
            display: flex;
        }

        button {
            margin-left: 5px;
            margin-right: 5px;
            padding: 6px 12px;
            font-size: 14px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;

            &.edit-btn {
                background-color: #ffcc00;
                color: white;

                &:hover {
                    background-color: #ffdd00;
                }
            }

            &.delete-btn {
                background-color: #f44336;
                color: white;

                &:hover {
                    background-color: #ff5b58;
                }
            }
        }
    }
}

/* 编辑区域的容器样式 */
.edit-container {
    padding: 20px;
    max-width: 600px;
    margin: 0 auto;
    background-color: #f9f9f9;
    border: 2px solid #ffee00;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

    h2 {
        font-size: 24px;
        margin-bottom: 20px;
        color: #333;
    }

    .el-form {
        display: flex;
        flex-direction: column;

        .el-form-item {
            margin-bottom: 15px;

            label {
                font-weight: bold;
                color: #555;
            }

            input {
                width: 100%;
                padding: 8px;
                border: 1px solid #ddd;
                border-radius: 4px;
                font-size: 14px;
                color: #333;
                outline: none;
                box-sizing: border-box;
                /* 包括内边距和边框 */

                &:focus {
                    border-color: #4caf50;
                    box-shadow: 0 0 4px rgba(76, 175, 80, 0.2);
                }
            }

            input[type="file"] {
                border: none;
                padding: 0;
            }
        }
    }

    button {
        padding: 10px 20px;
        font-size: 14px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        margin-right: 10px;
        transition: background-color 0.3s, color 0.3s;
        outline: none;

        &.submit-btn {
            background-color: #4caf50;
            color: white;

            &:hover {
                background-color: #45a049;
            }
        }

        &.cancel-btn {
            background-color: #f44336;
            color: white;

            &:hover {
                background-color: #e53935;
            }
        }
    }
}

.dishes-container {
    background-color: #ffd666;
    border: 2px solid black;
    border-radius: 20px;
    padding: 5px;

    max-height: 450px;
    /* 设置订单区域的最大高度 */
    display: flex;
    flex-direction: column;
    overflow-y: auto;
    /* 使订单区域可以滚动 */

    margin-right: 40px;
    margin-bottom: 10px;
}

.dishes-scroll {

    background-color: #ffd666;

    max-height: 430px;
    /* 设置订单区域的最大高度 */
    display: flex;
    flex-direction: column;
    overflow-y: auto;
    /* 使订单区域可以滚动 */
    margin-top: 5px;
    margin-bottom: 10px;

}

/* 隐藏滚动条 */
.dishes-scroll::-webkit-scrollbar {
    width: 12px;
}

/* 滚动条轨道 */
.dishes-scroll::-webkit-scrollbar-track {
    background: #ffd666;
}

/* 滚动条滑块 */
.dishes-scroll::-webkit-scrollbar-thumb {
    background-color: #ffd666;
    border-radius: 10px;
    border: 2px solid #000000;
}

.normal-button {
    margin-left: 5px;
    margin-right: 5px;
    padding: 6px 12px;
    font-size: 14px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.3s;
    background-color: #ffcc00;
    color: white;
}

.search-bar {
    display: flex;
    margin-bottom: 10px;
    margin-top: 5px;
}

.welcome-text {
    font-size: 35px;
    margin-left: 15px;
    color: #000000;
    font-weight: bold;
}
</style>
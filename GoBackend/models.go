// models.go
package main

import (
    "time"

    "gorm.io/gorm"
)

// ChatMessage represents a single message in a conversation.
type ChatMessage struct {
    ID             uint           `gorm:"primaryKey" json:"id"`
    ConversationKey string        `gorm:"type:text;not null" json:"conversation_key"`
    Role           string         `gorm:"type:text;not null" json:"role"`    // 'user', 'assistant', etc.
    Content        string         `gorm:"type:text;not null" json:"content"`
    CreatedAt      time.Time      `json:"created_at"`
    UpdatedAt      time.Time      `json:"updated_at"`
    DeletedAt      gorm.DeletedAt `gorm:"index" json:"-"`
}

